﻿using System.Collections.Generic;

namespace Classy.Models.Response
{
    public class PurchaseOptionView
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public Dictionary<string, string> VariantProperties { get; set; } // Key: Size, Color, Model, etc. Value: Smal, Medium, Large, etc.
        public string SKU { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal? CompareAtPrice { get; set; }
        public MediaFileView[] MediaFiles { get; set; }
        public string DefaultImage { get; set; }
        public string Height { get; set; }
        public string Width { get; set; }
        public string Depth { get; set; }
        public string ProductUrl { get; set; }
        public bool Available { get; set; }
        public string UID { get; set; }
    }
}
