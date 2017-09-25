using System;
using System.Collections.Generic;
using System.Linq;
using InSearch.Core.Data;
using InSearch.Core.Domain.Topics;
using InSearch.Core.Events;

namespace InSearch.Services.Topics
{
    /// <summary>
    /// Topic service
    /// </summary>
    public partial class TopicService : ITopicService
    {
        #region Fields

        private readonly IRepository<Topic> _topicRepository;
        private readonly IEventPublisher _eventPublisher;

        #endregion

        #region Ctor

        public TopicService(IRepository<Topic> topicRepository,
			IEventPublisher eventPublisher)
        {
            _topicRepository = topicRepository;
            _eventPublisher = eventPublisher;

			this.QuerySettings = DbQuerySettings.Default;
		}

		public DbQuerySettings QuerySettings { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Deletes a topic
        /// </summary>
        /// <param name="topic">Topic</param>
        public virtual void DeleteTopic(Topic topic)
        {
            if (topic == null)
                throw new ArgumentNullException("topic");

            _topicRepository.Delete(topic);

            //event notification
            _eventPublisher.EntityDeleted(topic);
        }

        /// <summary>
        /// Gets a topic
        /// </summary>
        /// <param name="topicId">The topic identifier</param>
        /// <returns>Topic</returns>
        public virtual Topic GetTopicById(int topicId)
        {
            if (topicId == 0)
                return null;

            return _topicRepository.GetById(topicId);
        }

        /// <summary>
        /// Gets a topic
        /// </summary>
        /// <param name="systemName">The topic system name</param>
        /// <returns>Topic</returns>
		public virtual Topic GetTopicBySystemName(string systemName)
        {
            if (String.IsNullOrEmpty(systemName))
                return null;

			var query = _topicRepository.Table;
			query = query.Where(t => t.SystemName == systemName);
			query = query.OrderBy(t => t.Id);

			return query.FirstOrDefault();
        }

        /// <summary>
        /// Gets all topics
        /// </summary>
        /// <returns>Topics</returns>
		public virtual IList<Topic> GetAllTopics()
        {
			var query = _topicRepository.Table;
			query = query.OrderBy(t => t.Priority).ThenBy(t => t.SystemName);

			return query.ToList();
        }

        /// <summary>
        /// Inserts a topic
        /// </summary>
        /// <param name="topic">Topic</param>
        public virtual void InsertTopic(Topic topic)
        {
            if (topic == null)
                throw new ArgumentNullException("topic");

            _topicRepository.Insert(topic);

            //event notification
            _eventPublisher.EntityInserted(topic);
        }

        /// <summary>
        /// Updates the topic
        /// </summary>
        /// <param name="topic">Topic</param>
        public virtual void UpdateTopic(Topic topic)
        {
            if (topic == null)
                throw new ArgumentNullException("topic");

            _topicRepository.Update(topic);

            //event notification
            _eventPublisher.EntityUpdated(topic);
        }

        #endregion
    }
}
