namespace CmsProject
{
    class Globals
    {
        public static CmsprojectDbConnection ctx;

        public static string phonePattern = (@"^((\(\d{3}\) ?)|(\d{3}-))?\d{3}-\d{4}$");

        public static string emailPattern = (@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");

        public static string codePostalPattern = (@"^[A-Za-z]\d[A-Za-z] ?\d[A-Za-z]\d$");
    }
}
