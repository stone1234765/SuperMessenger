﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SuperMessenger.Models.EntityFramework
{
    public class Invitation
    {
        public string Value { get; set; }
        public DateTime SendDate { get; set; }

        public Guid GroupId { get; set; }
        public Group Group { get; set; }
        public Guid InvitedUserId { get; set; }
        public ApplicationUser InvitedUser { get; set; }
        public Guid InviterId { get; set; }
        public ApplicationUser Inviter { get; set; }
    }
}
