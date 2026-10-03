namespace ATM.Core.Models
{
    public class AdminUser : User
    {
        public AdminUser()
        {
        }

        public AdminUser(List<ClientUser> requestedLoans, List<ClientUser> approvedLoans, List<ClientUser> rejectedLoans)
        {
            RequestedLoans = requestedLoans;
            ApprovedLoans = approvedLoans;
            RejectedLoans = rejectedLoans;
        }

        public List<ClientUser> RequestedLoans { get; set; }
        public List<ClientUser> ApprovedLoans { get; set; }
        public List<ClientUser> RejectedLoans { get; set; }
    }
}
