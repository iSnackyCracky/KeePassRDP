namespace KeePassRDP
{
    class AccountEntry
    {
        public AccountEntry(string path, string title, string username, string notes, int uidhash)
        {
            Path = path;
            Title = title;
            Username = username;
            Notes = notes;
            UidHash = uidhash;
        }
        public string Path { get; set; }
        public string Title { get; set; }
        public string Username { get; set; }
        public string Notes { get; set; }
        public int UidHash { get; set; }
    }
}
