﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using classy.Operations;
using Classy.Models;
using Classy.Models.Request;
using Classy.Models.Response;
using Classy.Repository;
using ServiceStack.Messaging;
using classy.Extentions;

namespace classy.Manager
{
    public class DefaultJobManager : IJobManager
    {
        private ManagerSecurityContext _context = null;
        private IJobRepository _jobsRepository = null;
        private IStorageRepository _storageRepository = null;
        private IMessageQueueClient _messageQueueClient = null;

        public Env Environment { get; set; }

        public DefaultJobManager(IJobRepository jobsRepository,
            IStorageRepository storageRepository,
            IMessageQueueClient messageQueueClient)
        {
            _jobsRepository = jobsRepository;
            _storageRepository = storageRepository;
            _messageQueueClient = messageQueueClient;
        }

        public JobView ScheduleCatalogImport(string appId, string profileId, bool overwriteListings, bool updateImages, byte[] catalog, string contentType, int catalogFormat)
        {
            string fileKey = Guid.NewGuid().ToString();
            //_storageRepository.SaveFile(fileKey, catalog, contentType);

            Job job = new Job
            {
                AppId = appId,
                ProfileId = profileId,
                Type = Job.JobType.ImportProductsCatalog,
                Status = "Not Started",
                CreatedAt = DateTime.UtcNow,
                Attachments = new MediaFile[] { new MediaFile {
                    ContentType = contentType,
                    Key = fileKey,
                    Type = MediaFileType.File
                } },
                Properties = new Dictionary<string, object> { { "CurrencyCode", Environment.CurrencyCode }, { "OverwriteListings", overwriteListings }, { "UpdateImages", updateImages }, { "CatalogFormat", catalogFormat } }
            };

            _jobsRepository.Save(job);
            var queueRequest = new ImportProductCatalogJob(job.Id, appId);
            //_messageQueueClient.Publish<ImportProductCatalogJob>(queueRequest);

            return new JobView { JobId = job.Id, Status = job.Status };
        }

        public ManagerSecurityContext SecurityContext
        {
            get
            {
                return _context;
            }
            set
            {
                _context = value;
            }
        }


        public IList<JobView> GetJobsStatus(string appId, string profileId)
        {
            return _jobsRepository.GetByProfileId(appId, profileId).ToJobViewList();
        }

        public string GetJobErrors(string appId, string jobId)
        {
            return string.Join("\r\n", _jobsRepository.GetById(appId, jobId).Errors);
        }
    }
}