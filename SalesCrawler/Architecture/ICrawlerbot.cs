﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesCrawler.Architecture
{
    interface ICrawlerbot
    {
        static Models.Crawlerbot Datasheet { get; }
    }
}
