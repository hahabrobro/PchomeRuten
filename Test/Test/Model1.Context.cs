﻿//------------------------------------------------------------------------------
// <auto-generated>
//    這個程式碼是由範本產生。
//
//    對這個檔案進行手動變更可能導致您的應用程式產生未預期的行為。
//    如果重新產生程式碼，將會覆寫對這個檔案的手動變更。
// </auto-generated>
//------------------------------------------------------------------------------

namespace Test
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class icshopc_icshopcEntities : DbContext
    {
        public icshopc_icshopcEntities()
            : base("name=icshopc_icshopcEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public DbSet<products> products { get; set; }
        public DbSet<products_description> products_description { get; set; }
        public DbSet<categories_description> categories_description { get; set; }
        public DbSet<products_to_categories> products_to_categories { get; set; }
        public DbSet<shipping_status> shipping_status { get; set; }
        public DbSet<categories> categories { get; set; }
    }
}