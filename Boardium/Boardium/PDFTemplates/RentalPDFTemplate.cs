using Boardium.Services;

namespace Boardium.PDFTemplates {
    public class RentalPDFTemplate {
        QrCodeService qrCodeService = new QrCodeService();
        public string? getHTMLRentalPDFTemplate(string InternalCode, string GameTitle, int PickupCode) {
            string qrCodeData = qrCodeService.GenerateQrCodeBase64(PickupCode.ToString());
            return $@"
                    <!DOCTYPE html>
                    <html>
                    <head>
                        <meta charset=""utf-8"">
                        <title>Rental Confirmation</title>
                        <style>
                            body {{
                                font-family: Arial, sans-serif;
                                padding: 40px;
                                line-height: 1.6;
                            }}
                            .container {{
                                max-width: 700px;
                                margin: auto;
                                border: 1px solid #ccc;
                                padding: 30px;
                                border-radius: 10px;
                            }}
                            h1 {{
                                color: #333;
                            }}
                            .info {{
                                margin-bottom: 20px;
                            }}
                            .label {{
                                font-weight: bold;
                            }}
                            .qr img {{
                                max-width: 200px;
                            }}
                        </style>
                    </head>
                    <body>
                        <div class=""container"">
                            <h1>Rental Confirmation</h1>
                    
                            <div class=""info"">
                                <p><span class=""label"">Pickup Code:</span> {PickupCode}</p>
                            </div>
                    
                            <div class=""info"">
                                <h2>Board Game Details</h2>
                                <p><span class=""label"">Title:</span> {GameTitle}</p>
                                <p><span class=""label"">Internal Code:</span> {InternalCode}</p>
                            </div>
                    
                            <p>Thank you for renting from <strong>Boardium</strong>!</p>

                            <div class=""qr"">
                                <h2>QR Code</h2>
                                <img src=""data:image/png;base64,{qrCodeData}"" alt=""QR Code"" />
                            </div>
                        </div>

                    </body>
                    </html>
                    ";
        }
    }
}
