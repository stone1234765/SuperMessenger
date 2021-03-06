﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SuperMessenger.Models.EntityFramework
{
    public class Message
    {
        public Guid Id { get; set; }
        public string Value { get; set; }
        public string OldValue { get; set; }
        public DateTime SendDate { get; set; }

        public Guid UserId { get; set; }
        public ApplicationUser User { get; set; }
        public Guid GroupId { get; set; }
        public Group Group { get; set; }
    }
}
