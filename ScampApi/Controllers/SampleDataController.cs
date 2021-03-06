﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DocumentDbRepositories;
using DocumentDbRepositories.Implementation;
using KeyVaultRepositories.Implementation;
using Microsoft.AspNet.Mvc;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace ScampApi.Controllers
{
    [Route("sampledata")]
    public class SampleDataController : Controller
    {
        private readonly IGroupRepository _groupRepository;
        private readonly IResourceRepository _resourceRepository;
        private readonly ISubscriptionRepository _subscriptionRepository;
        private readonly IUserRepository _userRepository;
        private readonly IKeyRepository _keyRepository;

        public SampleDataController(
            IUserRepository userRepository,
            IGroupRepository groupRepository,
            IResourceRepository resourceRepository,
            ISubscriptionRepository subscriptionRepository,
            IKeyRepository keyRepository)
        {
            _userRepository = userRepository;
            _groupRepository = groupRepository;
            _resourceRepository = resourceRepository;
            _subscriptionRepository = subscriptionRepository;
            _keyRepository = keyRepository;
        }
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        [ActionName("Index")]
        public async Task<IActionResult> Index_Post()
        {
            var subscriptionId = Request.Form["subscriptionId"];
            var subscriptionMngCert = Request.Form["subscriptionMngCert"];

            if (Request.Form["addSampleData"] == "on")
            {
                var user1 = new ScampUser
                {
                    Id = Guid.NewGuid().ToString("d"),
                    Name = "Some User1"
                };
                var user2 = new ScampUser
                {
                    Id = Guid.NewGuid().ToString("d"),
                    Name = "Some User2"
                };
                await _userRepository.CreateUser(user1);
                await _userRepository.CreateUser(user2);

                var group = new ScampResourceGroup
                {
                    Id = Guid.NewGuid().ToString("d"),
                    Name = "Classroom Sample",
                    Admins = new List<ScampUserReference> { user1 },
                    Members = new List<ScampUserReference> { user1, user2 },
                };
                await _groupRepository.CreateGroup(group);

                await _resourceRepository.CreateResource(new ScampResource
                {
                    Id = Guid.NewGuid().ToString("d"),
                    ResourceGroup = new ScampResourceGroupReference { Id = group.Id },
                    Name = "ScampDev",
                    ResourceType = ResourceType.VirtualMachine,
                    State = ResourceState.Allocated
                });
            }
            if (Request.Form["addSubscription"] == "on")
            {
                string id = Guid.NewGuid().ToString("d");
                await _subscriptionRepository.CreateSubscription(new ScampSubscription
                {
                    Id = id,
                    AzureSubscriptionID = subscriptionId,
                    AzureManagementThumbnail = "OnKeyVault"

                });
                _keyRepository.UpsertSecret(id, "cert", subscriptionMngCert);
            }
            return Content("Done!");
        }
    }
}
