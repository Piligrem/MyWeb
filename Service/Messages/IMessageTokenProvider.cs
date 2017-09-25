using System.Collections.Generic;
using InSearch.Core.Domain.Users;
using InSearch.Core.Domain.Messages;
using InSearch.Core.Domain.Forums;

namespace InSearch.Services.Messages
{
    public partial interface IMessageTokenProvider
    {
        void AddSiteTokens(IList<Token> tokens);
        void AddCompanyTokens(IList<Token> tokens);
        void AddContactDataTokens(IList<Token> tokens);
        void AddUserTokens(IList<Token> tokens, User user);
        void AddNewsLetterSubscriptionTokens(IList<Token> tokens, NewsLetterSubscription subscription);
        void AddBankConnectionTokens(IList<Token> tokens);

        #region Forum
        void AddForumTokens(IList<Token> tokens, Forum forum);
        void AddForumTopicTokens(IList<Token> tokens, ForumTopic forumTopic, int? friendlyForumTopicPageIndex = null, long? appendedPostIdentifierAnchor = null);
        void AddForumPostTokens(IList<Token> tokens, ForumPost forumPost);
        void AddPrivateMessageTokens(IList<Token> tokens, PrivateMessage privateMessage);
        #endregion Forum 

        string[] GetListOfAllowedTokens();
    }
}
