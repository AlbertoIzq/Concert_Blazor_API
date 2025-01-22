namespace Concert.Business.Constants
{
    public static class BackConstants
    {
        public const string API_LOGS_FILE_FULL_PATH = "../../../logs/api/Concert_api_logs_.log"; // Save outside of solution
        public const int API_LOGS_MAX_NUM_FILES = 21;

        // If new roles created, add them to AuthRegisterRolesFilterAttribute
        public const string READER_ROLE_ID = "9fc5f185-c6c3-4bcd-90c0-74e35304d69c";
        public const string WRITER_ROLE_ID = "5fb21249-08bc-4585-ae71-48392889955f";
        public const string ADMIN_ROLE_ID = "f3a2308a-3774-4882-8cab-e1b52ce0b48a";
        public const string READER_ROLE_NAME = "Reader";
        public const string WRITER_ROLE_NAME = "Writer";
        public const string ADMIN_ROLE_NAME = "Admin";

        public const int ACCESS_TOKEN_EXPIRATION_MINUTES = 15;
        public const int REFRESH_TOKEN_EXPIRATION_HOURS = 24; 
    }
}