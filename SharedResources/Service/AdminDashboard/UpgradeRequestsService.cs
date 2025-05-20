namespace DataAccess.Service
{
    using DataAccess.Model.AdminDashboard;
    using IRepository;
    using DataAccess.Service.AdminDashboard.Interfaces;
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
        }

        public async Task<List<UpgradeRequest>> RetrieveAllUpgradeRequests()
        {
            // First remove any requests from banned users
            await RemoveUpgradeRequestsFromBannedUsersAsync();
            
            // Then retrieve the updated list
            return await upgradeRequestsRepository.RetrieveAllUpgradeRequests();
        }

        public async Task RemoveUpgradeRequestsFromBannedUsersAsync()
        {
            List<UpgradeRequest> pendingUpgradeRequests = await upgradeRequestsRepository.RetrieveAllUpgradeRequests();

            foreach (var request in pendingUpgradeRequests)
            {
                RoleType roleType = await this.userRepository.GetRoleTypeForUser(request.RequestingUserIdentifier);

                if (roleType == RoleType.Banned)
                {
                    await this.upgradeRequestsRepository.RemoveUpgradeRequestByIdentifier(request.UpgradeRequestId);
                }
            }
        }

        public async Task<string> GetRoleNameBasedOnIdentifier(RoleType roleType)
        {
            List<Role> availableRoles = await this.rolesRepository.GetAllRoles();
            Role matchingRole = availableRoles.First(role => role.RoleType == roleType);
            return matchingRole.RoleName;
        }

        public async Task ProcessUpgradeRequest(bool isRequestAccepted, int upgradeRequestIdentifier)
        {
            if (isRequestAccepted)
            {
                UpgradeRequest currentUpgradeRequest = await this.upgradeRequestsRepository
                    .RetrieveUpgradeRequestByIdentifier(upgradeRequestIdentifier);

                if (currentUpgradeRequest == null)
                {
                    return;
                }

                Guid requestingUserIdentifier = currentUpgradeRequest.RequestingUserIdentifier;

                RoleType currentHighestRoleType = await this.userRepository
                    .GetRoleTypeForUser(requestingUserIdentifier);

                Role nextRoleLevel = await this.rolesRepository
                    .GetNextRoleInHierarchy(currentHighestRoleType);

                await this.userRepository
                    .ChangeRoleToUser(requestingUserIdentifier, nextRoleLevel);

                var user = await this.userRepository
                    .GetUserById(requestingUserIdentifier);

                if (user != null && !string.IsNullOrEmpty(user.EmailAddress))
                {
                    string smtpEmail = Environment.GetEnvironmentVariable("SMTP_MODERATOR_EMAIL");
                    string smtpPassword = Environment.GetEnvironmentVariable("SMTP_MODERATOR_PASSWORD");

                    // Only attempt to send email if credentials are available
                    if (!string.IsNullOrEmpty(smtpEmail) && !string.IsNullOrEmpty(smtpPassword))
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

                        DataAccess.Service.AdminDashboard.Components.SmtpEmailSender emailSender = new DataAccess.Service.AdminDashboard.Components.SmtpEmailSender();
                        await emailSender.SendEmailAsync(message, smtpEmail, smtpPassword);
                    }
                }
            }

            await this.upgradeRequestsRepository
                .RemoveUpgradeRequestByIdentifier(upgradeRequestIdentifier);
        }

        public async Task RemoveUpgradeRequestByIdentifier(int upgradeRequestIdentifier)
        {
            await this.upgradeRequestsRepository.RemoveUpgradeRequestByIdentifier(upgradeRequestIdentifier);
        }

        public async Task<UpgradeRequest> RetrieveUpgradeRequestByIdentifier(int upgradeRequestIdentifier)
        {
            return await this.upgradeRequestsRepository.RetrieveUpgradeRequestByIdentifier(upgradeRequestIdentifier);
        }
    }
}