using DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Contexts;

public partial class MpsDbContext : DbContext
{
    public MpsDbContext(DbContextOptions<MpsDbContext> options): base(options)
    {
    }

    public virtual DbSet<Agency> Agencies { get; set; }
    public virtual DbSet<AgencyCategory> AgencyCategories { get; set; }
    public virtual DbSet<Ethnicity> Ethnicities { get; set; }
    public virtual DbSet<Interface> Interfaces { get; set; }
    public virtual DbSet<Language> Languages { get; set; }
    public virtual DbSet<LoginHistory> LoginHistories { get; set; }
    public virtual DbSet<Module> Modules { get; set; }
    public virtual DbSet<ModuleInterfacePermission> ModuleInterfacePermissions { get; set; }
    public virtual DbSet<MpsPermission> Permissions { get; set; }
    public virtual DbSet<Role> Roles { get; set; }
    public virtual DbSet<RoleModuleInterfacePermission> RoleModuleInterfacePermissions { get; set; }
    public virtual DbSet<SettingsGridState> SettingsGridStates { get; set; }
    public virtual DbSet<MpsUser> Users { get; set; }
    public virtual DbSet<UserGroup> UserGroups { get; set; }
    public virtual DbSet<UserRole> UserRoles { get; set; }
    public virtual DbSet<UserMergeEmailLog> UserMergeEmailLogs { get; set; }
    public virtual DbSet<UserType> UserTypes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        
    }
}
