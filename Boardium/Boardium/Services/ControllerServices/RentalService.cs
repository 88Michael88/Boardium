using Boardium.Data;
using Boardium.HelperFuncs;
using Boardium.Models;
using Boardium.Models.Rental;
using Boardium.PDFTemplates;
using DinkToPdf;
using DinkToPdf.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Boardium.Services.ControllerServices {
    public class RentalService {
        private readonly BoardiumContext _context;
        private DateBetweenChecker _dateBetweenChecker;
        private PickupCodeGenerator _pickupCodeGenerator;
        private readonly IConverter _converter;

        public RentalService(BoardiumContext context, DateBetweenChecker dateBetweenChecker, PickupCodeGenerator pickupCodeGenerator, IConverter converter) {
            _context = context;
            _dateBetweenChecker = dateBetweenChecker;
            _pickupCodeGenerator = pickupCodeGenerator;
            _converter = converter;
        }
        public async Task<GameAvailableCopyDetailsViewModel?> GetGameCopyDetailsAsync(int GameID, int GameCopyID) {

            GameAvailableCopyDetailsViewModel? gameCopyDetail =
                await (from gc in _context.GameCopies
                       join g in _context.Games on gc.GameId equals g.Id
                       join r in _context.Rentals on gc.Id equals r.GameCopyId into rentalGroup
                       from rental in rentalGroup.DefaultIfEmpty() // LEFT JOIN
                       join gi in _context.GameImages on gc.GameId equals gi.GameId
                       where gc.GameId == GameID && gc.Id == GameCopyID
                       && gi.IsCoverImage == true
                       && rental.ReturnedAt == null
                       orderby rental.RentedAt
                       select new GameAvailableCopyDetailsViewModel {
                           GameCopyID = gc.Id,
                           GameID = gc.GameId,
                           Title = g.Title,
                           Condition = gc.Condition,
                           InventoryNumber = gc.InventoryNumber,
                           RentalFee = gc.RentalFee,
                           BorrowDate = rental.RentedAt,
                           DueDate = rental.DueDate,
                           CurrentBorrows = (from r in _context.Rentals
                                             where r.GameCopyId == GameCopyID
                                             && r.ReturnedAt == null
                                             && r.DueDate > DateTime.Now
                                             orderby r.RentedAt
                                             select new BorrowInfo {
                                                 BorrowDate = r.RentedAt,
                                                 DueDate = r.DueDate
                                             }
                                             ).ToList(),
                           PathToImage = gi.ImagePath
                       }).FirstOrDefaultAsync();

            return gameCopyDetail;
        }

        public async Task<decimal?> GetRentalFee(int GameID, int GameCopyID) {
            decimal? rentalFee = await (from gc in _context.GameCopies // Check if such a game exists.
                                        where gc.GameId == GameID && gc.Id == GameCopyID
                                        select gc.RentalFee
                                        ).FirstOrDefaultAsync();
            if (rentalFee == null) return null;

            return rentalFee;
        }

        public async Task<BorrowInfo[]> GetBorrowInfoAsync(int GameID, int GameCopyID, DateTime DesiredBorrowDate, DateTime DesiredDueDate) {
            BorrowInfo[] gameBorrowInfo = await (from gc in _context.GameCopies // Get all the current rentals of this game copy.
                                                 join r in _context.Rentals on gc.Id equals r.GameCopyId into rentalGroup
                                                 from rental in rentalGroup.DefaultIfEmpty() // LEFT JOIN
                                                 where rental.ReturnedAt == null
                                                 && rental.GameCopyId == GameCopyID
                                                 && rental.DueDate > DateTime.Now
                                                 select new BorrowInfo {
                                                     BorrowDate = rental.RentedAt,
                                                     DueDate = rental.DueDate,
                                                 }
                                          ).ToArrayAsync();
            return gameBorrowInfo;
        }

        public async Task<Rental> GetRentalConfirmationAsync(string userID, int GameID, int GameCopyID, DateTime DesiredBorrowDate, DateTime DesiredDueDate, decimal rentalFee) {
            var newRental = new Rental {
                GameCopyId = GameCopyID,
                ApplicationUserId = userID,
                RentedAt = DesiredBorrowDate,
                DueDate = DesiredDueDate,
                ReturnedAt = null,
                Status = RentalStatus.WaitingForAcceptance,
                Notes = "",
                RentalFee = rentalFee,
                LateFee = 0,
                DamageFee = 0,
                PaidFee = 0,
                PickupCode = _pickupCodeGenerator.GenerateCode(userID, DateTime.Now, GameCopyID)
            };

            _context.Rentals.Add(newRental);
            await _context.SaveChangesAsync();

            return newRental;
        }

        public async Task<List<ShowUserRentalData>> GetMyRentalInfoAsync(string userID) {

            var rentals = await (from r in _context.Rentals
                                 join gc in _context.GameCopies on r.GameCopyId equals gc.Id
                                 join g in _context.Games on gc.GameId equals g.Id
                                 where r.ApplicationUserId == userID
                                 select new ShowUserRentalData {
                                     GameTitle = g.Title,
                                     InventoryNumber = gc.InventoryNumber,
                                     RentedAt = r.RentedAt,
                                     DueDate = r.DueDate,
                                     ReturnedAt = r.ReturnedAt,
                                     Status = r.Status,
                                     RentalFee = r.RentalFee,
                                     LateFee = r.LateFee,
                                     DamageFee = r.DamageFee,
                                     PaidFee = r.PaidFee,
                                     PickupCode = r.PickupCode
                                 }
                                ).ToListAsync();

            return rentals;
        }

        public async Task<byte[]?> GeneratePDFAsync(string userID, int PickupCode) {
            RentalPDFTemplate rentalPDFTemplate = new RentalPDFTemplate();
            PDFDataModel? pdfDataModel = await (from r in _context.Rentals
                                               join gc in _context.GameCopies on r.GameCopyId equals gc.Id
                                               join g in _context.Games on gc.GameId equals g.Id
                                               where r.ApplicationUserId == userID
                                               && r.PickupCode == PickupCode
                                               select new PDFDataModel {
                                                   InventoryNumber = gc.InventoryNumber,
                                                   GameTitle = g.Title,
                                                   PickupCode = r.PickupCode
                                               }
                                              ).FirstOrDefaultAsync();


            if (pdfDataModel == null) return null;

            var htmlContent = rentalPDFTemplate.getHTMLRentalPDFTemplate(pdfDataModel.InventoryNumber, pdfDataModel.GameTitle, pdfDataModel.PickupCode);

            var doc = new HtmlToPdfDocument() {
                GlobalSettings = {
                    PaperSize = PaperKind.A5,
                    Orientation = Orientation.Portrait
                },
                Objects = {
                    new ObjectSettings() {
                        HtmlContent = htmlContent
                    }
                }
            };

            var pdf = _converter.Convert(doc);
            return pdf;
        }

    }
}
