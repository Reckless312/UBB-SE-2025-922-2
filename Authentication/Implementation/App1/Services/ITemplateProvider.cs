﻿// <copyright file="ITemplateProvider.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace App1.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public interface ITemplateProvider
    {
        public string GetEmailTemplate();

        public string GetPlainTextTemplate();

        public string GetReviewRowTemplate();
    }
}
