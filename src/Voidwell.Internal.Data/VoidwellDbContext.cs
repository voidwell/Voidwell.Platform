using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Voidwell.Internal.Data.Models;

namespace Voidwell.Internal.Data
{
    public class VoidwellDbContext: DbContext
    {
        public VoidwellDbContext(DbContextOptions<VoidwellDbContext> options)
            : base(options)
        {
        }

        public DbSet<BlogPost> BlogPosts { get; set; }
        public DbSet<CustomEvent> CustomEvents { get; set; }
        public DbSet<CustomEventTeam> CustomEventTeams { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            ApplyConfigurations(builder);
            ConvertToConvention(builder);
        }

        private static void ApplyConfigurations(ModelBuilder builder)
        {
            var applyGenericMethod = typeof(ModelBuilder).GetMethod("ApplyConfiguration", BindingFlags.Instance | BindingFlags.Public);
            foreach (var type in Assembly.GetExecutingAssembly().GetTypes().Where(c => c.IsClass && !c.IsAbstract && !c.ContainsGenericParameters))
            {
                foreach (var iface in type.GetInterfaces())
                {
                    if (iface.IsConstructedGenericType && iface.GetGenericTypeDefinition() == typeof(IEntityTypeConfiguration<>))
                    {
                        var applyConcreteMethod = applyGenericMethod.MakeGenericMethod(iface.GenericTypeArguments[0]);
                        applyConcreteMethod.Invoke(builder, new object[] { Activator.CreateInstance(type) });
                        break;
                    }
                }
            }
        }

        private static void ConvertToConvention(ModelBuilder builder)
        {
            foreach (var entity in builder.Model.GetEntityTypes())
            {
                // Replace table names
                entity.Relational().TableName = ToSnakeCase(entity.Relational().TableName);

                // Replace column names
                foreach (var property in entity.GetProperties())
                {
                    property.Relational().ColumnName = ToSnakeCase(property.Name);
                }

                foreach (var key in entity.GetKeys())
                {
                    var keyName = key.Relational().Name.Replace(Constants.EF.DefaultPrimaryKeyPrefix, "pk");
                    key.Relational().Name = ToSnakeCase(keyName);
                }

                foreach (var key in entity.GetForeignKeys())
                {
                    var keyName = key.Relational().Name.Replace(Constants.EF.DefaultForeignKeyPrefix, "fk");
                    key.Relational().Name = ToSnakeCase(keyName);
                }

                foreach (var index in entity.GetIndexes())
                {
                    var indexName = index.Relational().Name.Replace(Constants.EF.DefaultIndexPrefix, "ix");
                    index.Relational().Name = ToSnakeCase(indexName);
                }
            }
        }

        private static string ToSnakeCase(string input)
        {
            var result = Regex.Replace(input, ".[A-Z]", m => m.Value[0] + "_" + m.Value[1]);

            return result.ToLower();
        }
    }
}
