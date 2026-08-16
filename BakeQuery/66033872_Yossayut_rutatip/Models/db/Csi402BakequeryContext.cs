using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;

namespace _66033872_Yossayut_rutatip.Models.db;

public partial class Csi402BakequeryContext : DbContext
{
    public Csi402BakequeryContext()
    {
    }

    public Csi402BakequeryContext(DbContextOptions<Csi402BakequeryContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Cart> Carts { get; set; }

    public virtual DbSet<CartItem> CartItems { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<InventoryTransaction> InventoryTransactions { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderItem> OrderItems { get; set; }

    public virtual DbSet<OrderReply> OrderReplies { get; set; }

    public virtual DbSet<PaymentProof> PaymentProofs { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<Promotion> Promotions { get; set; }

    public virtual DbSet<PromotionRedemption> PromotionRedemptions { get; set; }

    public virtual DbSet<PromotionRule> PromotionRules { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserAddress> UserAddresses { get; set; }

    public virtual DbSet<LabStudent> LabStudents { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseMySql("server=localhost;port=3305;database=csi402_bakequery;user=root;password=Win220348", Microsoft.EntityFrameworkCore.ServerVersion.Parse("9.6.0-mysql"));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_general_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<Cart>(entity =>
        {
            entity.HasKey(e => e.CartId).HasName("PRIMARY");

            entity
                .ToTable("carts")
                .UseCollation("utf8mb4_0900_ai_ci");

            entity.HasIndex(e => new { e.UserId, e.Status }, "idx_carts_user_status");

            entity.Property(e => e.CartId).HasColumnName("cart_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'ACTIVE'")
                .HasColumnType("enum('ACTIVE','CHECKED_OUT','ABANDONED')")
                .HasColumnName("status");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.Carts)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("fk_carts_user");
        });

        modelBuilder.Entity<CartItem>(entity =>
        {
            entity.HasKey(e => e.CartItemId).HasName("PRIMARY");

            entity
                .ToTable("cart_items")
                .UseCollation("utf8mb4_0900_ai_ci");

            entity.HasIndex(e => e.ProductId, "idx_cart_items_product_id");

            entity.HasIndex(e => new { e.CartId, e.ProductId }, "uq_cart_items_cart_product").IsUnique();

            entity.Property(e => e.CartItemId).HasColumnName("cart_item_id");
            entity.Property(e => e.CartId).HasColumnName("cart_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.LineTotal)
                .HasPrecision(10, 2)
                .HasComputedColumnSql("`qty` * `unit_price`", true)
                .HasColumnName("line_total");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.Qty).HasColumnName("qty");
            entity.Property(e => e.UnitPrice)
                .HasPrecision(10, 2)
                .HasColumnName("unit_price");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Cart).WithMany(p => p.CartItems)
                .HasForeignKey(d => d.CartId)
                .HasConstraintName("fk_cart_items_cart");

            entity.HasOne(d => d.Product).WithMany(p => p.CartItems)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_cart_items_product");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PRIMARY");

            entity
                .ToTable("categories")
                .UseCollation("utf8mb4_0900_ai_ci");

            entity.HasIndex(e => e.CategoryCode, "uq_categories_code").IsUnique();

            entity.HasIndex(e => e.Name, "uq_categories_name").IsUnique();

            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.CategoryCode)
                .HasMaxLength(20)
                .HasColumnName("category_code");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .HasColumnName("description");
            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValueSql("'1'")
                .HasColumnName("is_active");
            entity.Property(e => e.Name)
                .HasMaxLength(80)
                .HasColumnName("name");
            entity.Property(e => e.SortOrder).HasColumnName("sort_order");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<InventoryTransaction>(entity =>
        {
            entity.HasKey(e => e.TransactionId).HasName("PRIMARY");

            entity
                .ToTable("inventory_transactions")
                .UseCollation("utf8mb4_0900_ai_ci");

            entity.HasIndex(e => e.CreatedBy, "fk_inventory_transactions_created_by");

            entity.HasIndex(e => e.CreatedAt, "idx_inventory_transactions_created_at");

            entity.HasIndex(e => e.ProductId, "idx_inventory_transactions_product_id");

            entity.Property(e => e.TransactionId).HasColumnName("transaction_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.Note)
                .HasMaxLength(255)
                .HasColumnName("note");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.QtyChange).HasColumnName("qty_change");
            entity.Property(e => e.ReferenceId).HasColumnName("reference_id");
            entity.Property(e => e.ReferenceType)
                .HasColumnType("enum('ORDER','MANUAL','RESTOCK')")
                .HasColumnName("reference_type");
            entity.Property(e => e.TransactionType)
                .HasColumnType("enum('IN','OUT','ADJUST')")
                .HasColumnName("transaction_type");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.InventoryTransactions)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_inventory_transactions_created_by");

            entity.HasOne(d => d.Product).WithMany(p => p.InventoryTransactions)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_inventory_transactions_product");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.OrderId).HasName("PRIMARY");

            entity
                .ToTable("orders")
                .UseCollation("utf8mb4_0900_ai_ci");

            entity.HasIndex(e => e.AddressId, "fk_orders_address");

            entity.HasIndex(e => e.PromotionId, "idx_orders_promotion_id");

            entity.HasIndex(e => new { e.OrderStatus, e.CreatedAt }, "idx_orders_status_created");

            entity.HasIndex(e => e.UserId, "idx_orders_user_id");

            entity.HasIndex(e => e.OrderNo, "uq_orders_order_no").IsUnique();

            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.AddressId).HasColumnName("address_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.DiscountTotal)
                .HasPrecision(10, 2)
                .HasColumnName("discount_total");
            entity.Property(e => e.GrandTotal)
                .HasPrecision(10, 2)
                .HasColumnName("grand_total");
            entity.Property(e => e.Notes)
                .HasMaxLength(500)
                .HasColumnName("notes");
            entity.Property(e => e.OrderNo)
                .HasMaxLength(30)
                .HasColumnName("order_no");
            entity.Property(e => e.OrderStatus)
                .HasDefaultValueSql("'PENDING'")
                .HasColumnType("enum('PENDING','PAID','PREPARING','SHIPPING','DELIVERED','CANCELLED')")
                .HasColumnName("order_status");
            entity.Property(e => e.PaymentStatus)
                .HasDefaultValueSql("'UNPAID'")
                .HasColumnType("enum('UNPAID','PAID','REFUNDED')")
                .HasColumnName("payment_status");
            entity.Property(e => e.PromotionId).HasColumnName("promotion_id");
            entity.Property(e => e.ShippingFee)
                .HasPrecision(10, 2)
                .HasColumnName("shipping_fee");
            entity.Property(e => e.Subtotal)
                .HasPrecision(10, 2)
                .HasColumnName("subtotal");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Address).WithMany(p => p.Orders)
                .HasForeignKey(d => d.AddressId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_orders_address");

            entity.HasOne(d => d.Promotion).WithMany(p => p.Orders)
                .HasForeignKey(d => d.PromotionId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_orders_promotion");

            entity.HasOne(d => d.User).WithMany(p => p.Orders)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_orders_user");
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => e.OrderItemId).HasName("PRIMARY");

            entity
                .ToTable("order_items")
                .UseCollation("utf8mb4_0900_ai_ci");

            entity.HasIndex(e => e.OrderId, "idx_order_items_order_id");

            entity.HasIndex(e => e.ProductId, "idx_order_items_product_id");

            entity.Property(e => e.OrderItemId).HasColumnName("order_item_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.LineTotal)
                .HasPrecision(10, 2)
                .HasComputedColumnSql("`qty` * `unit_price`", true)
                .HasColumnName("line_total");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.Qty).HasColumnName("qty");
            entity.Property(e => e.UnitPrice)
                .HasPrecision(10, 2)
                .HasColumnName("unit_price");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("fk_order_items_order");

            entity.HasOne(d => d.Product).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_order_items_product");
        });

        modelBuilder.Entity<OrderReply>(entity =>
        {
            entity.HasKey(e => e.ReplyId).HasName("PRIMARY");

            entity
                .ToTable("order_replies")
                .UseCollation("utf8mb4_0900_ai_ci");

            entity.HasIndex(e => e.OrderId, "idx_order_replies_order_id");

            entity.HasIndex(e => e.RepliedBy, "idx_order_replies_replied_by");

            entity.Property(e => e.ReplyId).HasColumnName("reply_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.RepliedBy).HasColumnName("replied_by");
            entity.Property(e => e.ReplyMessage)
                .HasMaxLength(1000)
                .HasColumnName("reply_message");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderReplies)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("fk_order_replies_order");

            entity.HasOne(d => d.RepliedByNavigation).WithMany(p => p.OrderReplies)
                .HasForeignKey(d => d.RepliedBy)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_order_replies_replied_by");
        });

        modelBuilder.Entity<PaymentProof>(entity =>
        {
            entity.HasKey(e => e.ProofId).HasName("PRIMARY");

            entity
                .ToTable("payment_proofs")
                .UseCollation("utf8mb4_0900_ai_ci");

            entity.HasIndex(e => e.OrderId, "idx_payment_proofs_order_id");

            entity.HasIndex(e => e.UploadedBy, "idx_payment_proofs_uploaded_by");

            entity.Property(e => e.ProofId).HasColumnName("proof_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.FilePath)
                .HasMaxLength(500)
                .HasColumnName("file_path");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.OriginalFileName)
                .HasMaxLength(255)
                .HasColumnName("original_file_name");
            entity.Property(e => e.UploadedBy).HasColumnName("uploaded_by");
            entity.Property(e => e.UploadNote)
                .HasMaxLength(500)
                .HasColumnName("upload_note");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
            entity.Property(e => e.VerificationStatus)
                .HasDefaultValueSql("'PENDING'")
                .HasColumnType("enum('PENDING','APPROVED','REJECTED')")
                .HasColumnName("verification_status");

            entity.HasOne(d => d.Order).WithMany(p => p.PaymentProofs)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("fk_payment_proofs_order");

            entity.HasOne(d => d.UploadedByNavigation).WithMany(p => p.PaymentProofs)
                .HasForeignKey(d => d.UploadedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_payment_proofs_uploaded_by");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.ProductId).HasName("PRIMARY");

            entity
                .ToTable("products")
                .UseCollation("utf8mb4_0900_ai_ci");

            entity.HasIndex(e => e.CreatedBy, "fk_products_created_by");

            entity.HasIndex(e => e.UpdatedBy, "fk_products_updated_by");

            entity.HasIndex(e => e.CategoryId, "idx_products_category_id");

            entity.HasIndex(e => e.Status, "idx_products_status");

            entity.HasIndex(e => e.ProductCode, "uq_products_code").IsUnique();

            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.Description)
                .HasColumnType("text")
                .HasColumnName("description");
            entity.Property(e => e.ImageUrl)
                .HasMaxLength(500)
                .HasColumnName("image_url");
            entity.Property(e => e.Name)
                .HasMaxLength(120)
                .HasColumnName("name");
            entity.Property(e => e.Price)
                .HasPrecision(10, 2)
                .HasColumnName("price");
            entity.Property(e => e.ProductCode)
                .HasMaxLength(20)
                .HasColumnName("product_code");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'ACTIVE'")
                .HasColumnType("enum('ACTIVE','OUT_OF_STOCK','INACTIVE')")
                .HasColumnName("status");
            entity.Property(e => e.StockQty).HasColumnName("stock_qty");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
            entity.Property(e => e.UpdatedBy).HasColumnName("updated_by");

            entity.HasOne(d => d.Category).WithMany(p => p.Products)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_products_category");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.ProductCreatedByNavigations)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_products_created_by");

            entity.HasOne(d => d.UpdatedByNavigation).WithMany(p => p.ProductUpdatedByNavigations)
                .HasForeignKey(d => d.UpdatedBy)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_products_updated_by");
        });

        modelBuilder.Entity<Promotion>(entity =>
        {
            entity.HasKey(e => e.PromotionId).HasName("PRIMARY");

            entity
                .ToTable("promotions")
                .UseCollation("utf8mb4_0900_ai_ci");

            entity.HasIndex(e => e.CreatedBy, "fk_promotions_created_by");

            entity.HasIndex(e => e.UpdatedBy, "fk_promotions_updated_by");

            entity.HasIndex(e => new { e.IsActive, e.StartAt, e.EndAt }, "idx_promotions_active_period");

            entity.HasIndex(e => e.PromoCode, "uq_promotions_code").IsUnique();

            entity.Property(e => e.PromotionId).HasColumnName("promotion_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.Description)
                .HasColumnType("text")
                .HasColumnName("description");
            entity.Property(e => e.EndAt)
                .HasColumnType("datetime")
                .HasColumnName("end_at");
            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValueSql("'1'")
                .HasColumnName("is_active");
            entity.Property(e => e.Name)
                .HasMaxLength(150)
                .HasColumnName("name");
            entity.Property(e => e.PromoCode)
                .HasMaxLength(30)
                .HasColumnName("promo_code");
            entity.Property(e => e.PromoType)
                .HasColumnType("enum('PERCENT','FIXED','BUY_X_GET_Y','MEMBER')")
                .HasColumnName("promo_type");
            entity.Property(e => e.StartAt)
                .HasColumnType("datetime")
                .HasColumnName("start_at");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
            entity.Property(e => e.UpdatedBy).HasColumnName("updated_by");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.PromotionCreatedByNavigations)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_promotions_created_by");

            entity.HasOne(d => d.UpdatedByNavigation).WithMany(p => p.PromotionUpdatedByNavigations)
                .HasForeignKey(d => d.UpdatedBy)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_promotions_updated_by");
        });

        modelBuilder.Entity<PromotionRedemption>(entity =>
        {
            entity.HasKey(e => e.RedemptionId).HasName("PRIMARY");

            entity
                .ToTable("promotion_redemptions")
                .UseCollation("utf8mb4_0900_ai_ci");

            entity.HasIndex(e => e.OrderId, "idx_promotion_redemptions_order_id");

            entity.HasIndex(e => e.PromotionId, "idx_promotion_redemptions_promotion_id");

            entity.HasIndex(e => e.UserId, "idx_promotion_redemptions_user_id");

            entity.Property(e => e.RedemptionId).HasColumnName("redemption_id");
            entity.Property(e => e.DiscountValue)
                .HasPrecision(10, 2)
                .HasColumnName("discount_value");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.PromotionId).HasColumnName("promotion_id");
            entity.Property(e => e.RedeemedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("redeemed_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Order).WithMany(p => p.PromotionRedemptions)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("fk_promotion_redemptions_order");

            entity.HasOne(d => d.Promotion).WithMany(p => p.PromotionRedemptions)
                .HasForeignKey(d => d.PromotionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_promotion_redemptions_promotion");

            entity.HasOne(d => d.User).WithMany(p => p.PromotionRedemptions)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_promotion_redemptions_user");
        });

        modelBuilder.Entity<PromotionRule>(entity =>
        {
            entity.HasKey(e => e.RuleId).HasName("PRIMARY");

            entity
                .ToTable("promotion_rules")
                .UseCollation("utf8mb4_0900_ai_ci");

            entity.HasIndex(e => e.FreeProductId, "idx_promotion_rules_free_product_id");

            entity.HasIndex(e => e.PromotionId, "idx_promotion_rules_promotion_id");

            entity.Property(e => e.RuleId).HasColumnName("rule_id");
            entity.Property(e => e.BuyQty).HasColumnName("buy_qty");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.DiscountAmount)
                .HasPrecision(10, 2)
                .HasColumnName("discount_amount");
            entity.Property(e => e.DiscountPercent)
                .HasPrecision(5, 2)
                .HasColumnName("discount_percent");
            entity.Property(e => e.FreeProductId).HasColumnName("free_product_id");
            entity.Property(e => e.FreeQty).HasColumnName("free_qty");
            entity.Property(e => e.MaxRedemptions).HasColumnName("max_redemptions");
            entity.Property(e => e.MaxRedemptionsPerUser).HasColumnName("max_redemptions_per_user");
            entity.Property(e => e.MemberOnly).HasColumnName("member_only");
            entity.Property(e => e.MinOrderAmount)
                .HasPrecision(10, 2)
                .HasColumnName("min_order_amount");
            entity.Property(e => e.PromotionId).HasColumnName("promotion_id");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.FreeProduct).WithMany(p => p.PromotionRules)
                .HasForeignKey(d => d.FreeProductId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_promotion_rules_free_product");

            entity.HasOne(d => d.Promotion).WithMany(p => p.PromotionRules)
                .HasForeignKey(d => d.PromotionId)
                .HasConstraintName("fk_promotion_rules_promotion");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PRIMARY");

            entity
                .ToTable("roles")
                .UseCollation("utf8mb4_0900_ai_ci");

            entity.HasIndex(e => e.RoleName, "uq_roles_role_name").IsUnique();

            entity.Property(e => e.RoleId)
                .ValueGeneratedOnAdd()
                .HasColumnName("role_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .HasColumnName("description");
            entity.Property(e => e.RoleName)
                .HasMaxLength(30)
                .HasColumnName("role_name");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PRIMARY");

            entity
                .ToTable("users")
                .UseCollation("utf8mb4_0900_ai_ci");

            entity.HasIndex(e => e.RoleId, "idx_users_role_id");

            entity.HasIndex(e => e.Email, "uq_users_email").IsUnique();

            entity.HasIndex(e => e.UserCode, "uq_users_user_code").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Email)
                .HasMaxLength(120)
                .HasColumnName("email");
            entity.Property(e => e.FullName)
                .HasMaxLength(120)
                .HasColumnName("full_name");
            entity.Property(e => e.LastLoginAt)
                .HasColumnType("datetime")
                .HasColumnName("last_login_at");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .HasColumnName("password_hash");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .HasColumnName("phone");
            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'ACTIVE'")
                .HasColumnType("enum('ACTIVE','INACTIVE','BANNED')")
                .HasColumnName("status");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserCode)
                .HasMaxLength(20)
                .HasColumnName("user_code");

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_users_role");
        });

        modelBuilder.Entity<UserAddress>(entity =>
        {
            entity.HasKey(e => e.AddressId).HasName("PRIMARY");

            entity
                .ToTable("user_addresses")
                .UseCollation("utf8mb4_0900_ai_ci");

            entity.HasIndex(e => e.UserId, "idx_user_addresses_user_id");

            entity.Property(e => e.AddressId).HasColumnName("address_id");
            entity.Property(e => e.Country)
                .HasMaxLength(80)
                .HasDefaultValueSql("'Thailand'")
                .HasColumnName("country");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.District)
                .HasMaxLength(120)
                .HasColumnName("district");
            entity.Property(e => e.IsDefault).HasColumnName("is_default");
            entity.Property(e => e.Line1)
                .HasMaxLength(255)
                .HasColumnName("line1");
            entity.Property(e => e.Line2)
                .HasMaxLength(255)
                .HasColumnName("line2");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .HasColumnName("phone");
            entity.Property(e => e.PostalCode)
                .HasMaxLength(10)
                .HasColumnName("postal_code");
            entity.Property(e => e.Province)
                .HasMaxLength(120)
                .HasColumnName("province");
            entity.Property(e => e.RecipientName)
                .HasMaxLength(120)
                .HasColumnName("recipient_name");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.UserAddresses)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("fk_user_addresses_user");
        });

        modelBuilder.Entity<LabStudent>(entity =>
        {
            entity.HasKey(e => e.StdID);
            entity.ToTable("LabStudent");
            entity.Property(e => e.StdID);
            entity.Property(e => e.StdPASSWORD);
            entity.Property(e => e.StdName);
            entity.Property(e => e.StdLastname);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
