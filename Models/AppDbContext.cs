using  AppMvc.Models.Blog;
using AppMvc.Models.Contacts;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AppMvc.Models{



    public class AppDbContext : IdentityDbContext<AppUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            //bỏ tiền tố AspNet
            // lấy type
            foreach (var entitytype in builder.Model.GetEntityTypes())
            {
                var tableName = entitytype.GetTableName();
                if (tableName.StartsWith("AspNet"))
                {
                    entitytype.SetTableName(tableName.Substring(6));
                }

            }
            builder.Entity<Category>(entity=>{
                entity.HasIndex(c=>c.Slug);
            });
            builder.Entity<PostCategory>(entity=>{
                entity.HasKey(c=> new {c.PostID,c.CategoryID});
            });
            builder.Entity<Post>(entity=>{
                entity.HasIndex(c=>c.Slug).IsUnique();
            });
        }
        public DbSet<Contact> Contacts {set;get;}
        public DbSet<Category> Categories {set;get;}

        public DbSet<Post> Posts {set; get;}
        public DbSet<PostCategory> PostCategories {set; get;}
    }

}