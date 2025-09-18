namespace PrintingTools.Domain.Constants;

public static class FileTypes
{
    public static class Documents
    {
        public const string Pdf = "pdf";
        public const string Doc = "doc";
        public const string Docx = "docx";
        public const string Odt = "odt";
        public const string Rtf = "rtf";
        public const string Txt = "txt";
    }

    public static class Spreadsheets
    {
        public const string Xls = "xls";
        public const string Xlsx = "xlsx";
        public const string Ods = "ods";
        public const string Csv = "csv";
    }

    public static class Images
    {
        public const string Jpg = "jpg";
        public const string Jpeg = "jpeg";
        public const string Png = "png";
        public const string Bmp = "bmp";
        public const string Tiff = "tiff";
        public const string Gif = "gif";
    }

    public static class Cad
    {
        public const string Dwg = "dwg";
        public const string Dxf = "dxf";
        public const string Plt = "plt";
        public const string Hpgl = "hpgl";
    }

    public static bool IsSupported(string fileType)
    {
        if (string.IsNullOrWhiteSpace(fileType))
            return false;

        var type = fileType.ToLowerInvariant().TrimStart('.');
        
        return IsDocument(type) || IsSpreadsheet(type) || IsImage(type) || IsCad(type);
    }

    public static bool IsDocument(string fileType)
    {
        var type = fileType.ToLowerInvariant().TrimStart('.');
        return type is Documents.Pdf or Documents.Doc or Documents.Docx or Documents.Odt or Documents.Rtf or Documents.Txt;
    }

    public static bool IsSpreadsheet(string fileType)
    {
        var type = fileType.ToLowerInvariant().TrimStart('.');
        return type is Spreadsheets.Xls or Spreadsheets.Xlsx or Spreadsheets.Ods or Spreadsheets.Csv;
    }

    public static bool IsImage(string fileType)
    {
        var type = fileType.ToLowerInvariant().TrimStart('.');
        return type is Images.Jpg or Images.Jpeg or Images.Png or Images.Bmp or Images.Tiff or Images.Gif;
    }

    public static bool IsCad(string fileType)
    {
        var type = fileType.ToLowerInvariant().TrimStart('.');
        return type is Cad.Dwg or Cad.Dxf or Cad.Plt or Cad.Hpgl;
    }

    public static bool RequiresConversion(string fileType)
    {
        var type = fileType.ToLowerInvariant().TrimStart('.');
        return type != Documents.Pdf && type != Images.Jpg && type != Images.Jpeg && type != Images.Png;
    }
}