/*
 *  DB Context.
 *  TODO: inject class with app settings like the @connetionString
 */

namespace LightCMS.Config
{
    using Microsoft.EntityFrameworkCore;
    using MySQL.Data.EntityFrameworkCore.Extensions;
    using Components.Main.Models;
    using Components.Main.User.Models;

    public class CMSDBContext : DbContext
    {
        public CMSDBContext(DbContextOptions<CMSDBContext> options)
        : base(options)
        { }

        //TODO: move to components, key idea : using partial classes maybe ?
        public DbSet<MenuItem> MenuItems { get; set; }
        public DbSet<MenuItemType> MenuItemTypes { get; set; }
        public DbSet<Menu> Menus { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<Extension> Extensions { get; set; }
        public DbSet<Language> Language { get; set; }
        public DbSet<Item_Language> Item_Language { get; set; }
        public DbSet<Category_Language> Category_Language { get; set; }
        public DbSet<MenuItem_Language> MenuItem_Language { get; set; }
        public DbSet<Menu_Language> Menu_Language { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<Role> Roles { get; set; }

    }

        public static class CMSContextFactory
        {
            public static CMSDBContext Create(string connectionString)
            {
                var optionsBuilder = new DbContextOptionsBuilder<CMSDBContext>();
                optionsBuilder.UseMySQL(connectionString);

                //Ensure database creation
                var context = new CMSDBContext(optionsBuilder.Options);
                context.Database.EnsureCreated();

                return context; 
            }
        }
}