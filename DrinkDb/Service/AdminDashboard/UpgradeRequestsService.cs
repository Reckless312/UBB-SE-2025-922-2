namespace DrinkDb_Auth.Service
{
    using DataAccess.Model.AdminDashboard;
    using IRepository;
    using DrinkDb_Auth.Service.AdminDashboard.Interfaces;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using MimeKit;
    using System.Threading;

    public class UpgradeRequestsService : IUpgradeRequestsService
    {
        private readonly IUpgradeRequestsRepository upgradeRequestsRepository;
        private readonly IRolesRepository rolesRepository;
        private readonly IUserRepository userRepository;

        public UpgradeRequestsService(
            IUpgradeRequestsRepository upgradeRequestsRepository,
            IRolesRepository rolesRepository,
            IUserRepository userRepository)
        {
            this.upgradeRequestsRepository = upgradeRequestsRepository;
            this.rolesRepository = rolesRepository;
            this.userRepository = userRepository;
            this.RemoveUpgradeRequestsFromBannedUsers();
        }

        public void RemoveUpgradeRequestsFromBannedUsers()
        {
            List<UpgradeRequest> pendingUpgradeRequests = this.RetrieveAllUpgradeRequests();

            // Use a reversed loop or a copy of the list to safely remove items
            for (int i = pendingUpgradeRequests.Count - 1; i >= 0; i--)
            {
                Guid requestingUserIdentifier = pendingUpgradeRequests[i].RequestingUserIdentifier;

                // Avoid blocking by using ConfigureAwait(false) but still getting the result
                RoleType roleType = this.userRepository.GetRoleTypeForUser(requestingUserIdentifier)
                    .ConfigureAwait(false).GetAwaiter().GetResult();

                if (roleType == RoleType.Banned)
                {
                    this.upgradeRequestsRepository.RemoveUpgradeRequestByIdentifier(pendingUpgradeRequests[i].UpgradeRequestId)
                        .ConfigureAwait(false).GetAwaiter().GetResult();
                }
            }
        }

        public string GetRoleNameBasedOnIdentifier(RoleType roleType)
        {
            List<Role> availableRoles = this.rolesRepository.GetAllRoles()
                .ConfigureAwait(false).GetAwaiter().GetResult();
            Role matchingRole = availableRoles.First(role => role.RoleType == roleType);
            return matchingRole.RoleName;
        }

        public List<UpgradeRequest> RetrieveAllUpgradeRequests()
        {
            return this.upgradeRequestsRepository.RetrieveAllUpgradeRequests()
                .ConfigureAwait(false).GetAwaiter().GetResult();
        }

        // Implement async version for internal use and future compatibility
        internal async Task ProcessUpgradeRequestAsync(bool isRequestAccepted, int upgradeRequestIdentifier)
        {
            if (isRequestAccepted)
            {
                UpgradeRequest currentUpgradeRequest = await this.upgradeRequestsRepository
                    .RetrieveUpgradeRequestByIdentifier(upgradeRequestIdentifier)
                    .ConfigureAwait(false);

                Guid requestingUserIdentifier = currentUpgradeRequest.RequestingUserIdentifier;

                RoleType currentHighestRoleType = await this.userRepository
                    .GetRoleTypeForUser(requestingUserIdentifier)
                    .ConfigureAwait(false);

                Role nextRoleLevel = await this.rolesRepository
                    .GetNextRoleInHierarchy(currentHighestRoleType)
                    .ConfigureAwait(false);

                await this.userRepository
                    .ChangeRoleToUser(requestingUserIdentifier, nextRoleLevel)
                    .ConfigureAwait(false);

                // --- Always fetch the latest user info from the database ---
                var user = await this.userRepository
                    .GetUserById(requestingUserIdentifier)
                    .ConfigureAwait(false);

                if (user != null && !string.IsNullOrEmpty(user.EmailAddress))
                {
                    string htmlBody = $@"
                 <html>
                   <body>
                     <h2>Congratulations!</h2>
                     <p>Your account has been upgraded.</p>
                     <p>
                       <b>Username:</b> {user.Username}<br>
                       <b>New Role:</b> {nextRoleLevel.RoleName}<br>
                       <b>Date:</b> {DateTime.Now:yyyy-MM-dd HH:mm}
                     </p>
                     <p>Thank you for being part of our community!</p>
                   </body>
                 </html>";

                    MimeMessage message = new MimeMessage();
                    message.From.Add(new MailboxAddress("System Admin", "your-admin-email@example.com"));
                    message.To.Add(new MailboxAddress(user.Username, user.EmailAddress));
                    message.Subject = "Your Account Has Been Upgraded!";
                    MimeKit.BodyBuilder bodyBuilder = new BodyBuilder { HtmlBody = htmlBody };
                    message.Body = bodyBuilder.ToMessageBody();

                    // Use your existing email sender service
                    DrinkDb_Auth.Service.AdminDashboard.Components.SmtpEmailSender emailSender = new DrinkDb_Auth.Service.AdminDashboard.Components.SmtpEmailSender();
                    // Replace with your SMTP credentials
                    System.String smtpEmail = Environment.GetEnvironmentVariable("SMTP_MODERATOR_EMAIL");
                    System.String smtpPassword = Environment.GetEnvironmentVariable("SMTP_MODERATOR_PASSWORD");

                    await emailSender.SendEmailAsync(message, smtpEmail, smtpPassword).ConfigureAwait(false);
                }
            }

            await this.upgradeRequestsRepository
                .RemoveUpgradeRequestByIdentifier(upgradeRequestIdentifier)
                .ConfigureAwait(false);
        }

       
        public void ProcessUpgradeRequest(bool isRequestAccepted, int upgradeRequestIdentifier)
        {
            // Run the async method on a separate thread to avoid deadlocks
            Task.Run(() =>
            {
                ProcessUpgradeRequestAsync(isRequestAccepted, upgradeRequestIdentifier).Wait();
            }).Wait();
        }

        public async Task RemoveUpgradeRequestByIdentifierAsync(int upgradeRequestIdentifier)
        {
            await this.upgradeRequestsRepository.RemoveUpgradeRequestByIdentifier(upgradeRequestIdentifier);
        }

        public async Task<UpgradeRequest> RetrieveUpgradeRequestByIdentifierAsync(int upgradeRequestIdentifier)
        {
            return await this.upgradeRequestsRepository.RetrieveUpgradeRequestByIdentifier(upgradeRequestIdentifier);
        }
    }
}