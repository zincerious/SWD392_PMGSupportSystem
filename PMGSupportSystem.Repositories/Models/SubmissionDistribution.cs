﻿    // <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
    #nullable disable
    using System;
    using System.Collections.Generic;

    namespace PMGSupportSystem.Repositories.Models;

    public partial class SubmissionDistribution
    {
        public Guid ExamDistributionId { get; set; }

        public Guid? SubmissionId { get; set; }

        public Guid? LecturerId { get; set; }

        public DateTime? AssignedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public DateTime? Deadline { get; set; }

        public string Status { get; set; }

        public virtual User Lecturer { get; set; }

        public virtual Submission Submission { get; set; }
    }