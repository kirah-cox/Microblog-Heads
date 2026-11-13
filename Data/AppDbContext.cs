using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using microblog_heads.Models;

namespace microblog_heads.Data;

public partial class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }
    public virtual DbSet<CommunityJoinRequest> CommunityJoinRequests { get; set; }

    public virtual DbSet<BlockedUser> BlockedUsers { get; set; }

    public virtual DbSet<Comment> Comments { get; set; }

    public virtual DbSet<CommentLike> CommentLikes { get; set; }

    public virtual DbSet<CommentMissuse> CommentMissuses { get; set; }

    public virtual DbSet<Community> Communities { get; set; }

    public virtual DbSet<CommunityRole> CommunityRoles { get; set; }

    public virtual DbSet<CommunityUserRole> CommunityUserRoles { get; set; }

    public virtual DbSet<Follower> Followers { get; set; }

    public virtual DbSet<HashTag> HashTags { get; set; }

    public virtual DbSet<HashTagFollow> HashTagFollows { get; set; }

    public virtual DbSet<Media> Media { get; set; }

    public virtual DbSet<Post> Posts { get; set; }

    public virtual DbSet<PostFollow> PostFollows { get; set; }

    public virtual DbSet<PostHashTag> PostHashTags { get; set; }

    public virtual DbSet<PostLike> PostLikes { get; set; }

    public virtual DbSet<PostMedia> PostMedia { get; set; }

    public virtual DbSet<PostMissuse> PostMissuses { get; set; }

    public virtual DbSet<RegularUser> RegularUsers { get; set; }

    public virtual DbSet<UserRole> UserRoles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CommunityJoinRequest>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("community_join_request_pkey");

            entity.ToTable("community_join_request", "Microblog");

            entity.HasIndex(e => new { e.CommunityId, e.RegularUserId }, "uq_join_request").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CommunityId).HasColumnName("community_id");
            entity.Property(e => e.RegularUserId).HasColumnName("regular_user_id");
            entity.Property(e => e.DateRequested)
                .HasColumnName("date_requested")
                .HasColumnType("timestamp without time zone");

            entity.HasOne(d => d.Community).WithMany(p => p.CommunityJoinRequests)
                .HasForeignKey(d => d.CommunityId)
                .HasConstraintName("fk_join_request_community");

            entity.HasOne(d => d.RegularUser).WithMany(p => p.CommunityJoinRequests)
                .HasForeignKey(d => d.RegularUserId)
                .HasConstraintName("fk_join_request_user");
        });
        modelBuilder.Entity<BlockedUser>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("blocked_users_pkey");

            entity.ToTable("blocked_users", "Microblog");

            entity.HasIndex(e => new { e.RegularUserBlockingId, e.RegularUserBlockedId }, "uq_blocked_users").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.RegularUserBlockedId).HasColumnName("regular_user_blocked_id");
            entity.Property(e => e.RegularUserBlockingId).HasColumnName("regular_user_blocking_id");

            entity.HasOne(d => d.RegularUserBlocked).WithMany(p => p.BlockedUserRegularUserBlockeds)
                .HasForeignKey(d => d.RegularUserBlockedId)
                .HasConstraintName("fk_blocked_users_blocked");

            entity.HasOne(d => d.RegularUserBlocking).WithMany(p => p.BlockedUserRegularUserBlockings)
                .HasForeignKey(d => d.RegularUserBlockingId)
                .HasConstraintName("fk_blocked_users_blocking");
        });

        modelBuilder.Entity<Comment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("comment_pkey");

            entity.ToTable("comment", "Microblog");

            entity.HasIndex(e => new { e.Id, e.PostId }, "uq_comment_id_post").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Content)
                .HasMaxLength(300)
                .HasColumnName("content");
            entity.Property(e => e.ParentCommentId).HasColumnName("parent_comment_id");
            entity.Property(e => e.PostId).HasColumnName("post_id");
            entity.Property(e => e.RegularUserId).HasColumnName("regular_user_id");
            entity.Property(e => e.DatePosted).HasColumnName("date_posted").HasColumnType("timestamp without time zone");

            entity.HasOne(d => d.Post).WithMany(p => p.Comments)
                .HasForeignKey(d => d.PostId)
                .HasConstraintName("fk_comment_post");

            entity.HasOne(d => d.RegularUser).WithMany(p => p.Comments)
                .HasForeignKey(d => d.RegularUserId)
                .HasConstraintName("fk_comment_user");

            entity.HasOne(d => d.CommentNavigation).WithMany(p => p.InverseCommentNavigation)
                .HasPrincipalKey(p => new { p.Id, p.PostId })
                .HasForeignKey(d => new { d.ParentCommentId, d.PostId })
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_comment_parent_same_post");
        });

        modelBuilder.Entity<CommentLike>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("comment_like_pkey");

            entity.ToTable("comment_like", "Microblog");

            entity.HasIndex(e => new { e.RegularUserId, e.CommentId }, "uq_comment_like").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CommentId).HasColumnName("comment_id");
            entity.Property(e => e.RegularUserId).HasColumnName("regular_user_id");

            entity.HasOne(d => d.Comment).WithMany(p => p.CommentLikes)
                .HasForeignKey(d => d.CommentId)
                .HasConstraintName("fk_comment_like_comment");

            entity.HasOne(d => d.RegularUser).WithMany(p => p.CommentLikes)
                .HasForeignKey(d => d.RegularUserId)
                .HasConstraintName("fk_comment_like_user");
        });

        modelBuilder.Entity<CommentMissuse>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("comment_missuse_pkey");

            entity.ToTable("comment_missuse", "Microblog");

            entity.HasIndex(e => new { e.RegularUserId, e.CommentId, e.MissuseName }, "uq_comment_missuse").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CommentId).HasColumnName("comment_id");
            entity.Property(e => e.MissuseDescription)
                .HasMaxLength(200)
                .HasColumnName("missuse_description");
            entity.Property(e => e.MissuseName)
                .HasMaxLength(50)
                .HasColumnName("missuse_name");
            entity.Property(e => e.RegularUserId).HasColumnName("regular_user_id");

            entity.HasOne(d => d.Comment).WithMany(p => p.CommentMissuses)
                .HasForeignKey(d => d.CommentId)
                .HasConstraintName("fk_comment_missuse_comment");

            entity.HasOne(d => d.RegularUser).WithMany(p => p.CommentMissuses)
                .HasForeignKey(d => d.RegularUserId)
                .HasConstraintName("fk_comment_missuse_user");
        });

        modelBuilder.Entity<Community>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("community_pkey");

            entity.ToTable("community", "Microblog");

            entity.HasIndex(e => e.Name, "uq_community_name").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description)
                .HasMaxLength(200)
                .HasColumnName("description");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
            entity.Property(e => e.ParentCommunityId).HasColumnName("parent_community_id");
            entity.Property(e => e.IsPrivate)
                .HasDefaultValue(false)
                .HasColumnName("is_private");

            entity.HasOne(d => d.ParentCommunity).WithMany(p => p.InverseParentCommunity)
                .HasForeignKey(d => d.ParentCommunityId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_community_parent");
        });

        modelBuilder.Entity<CommunityRole>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("community_role_pkey");

            entity.ToTable("community_role", "Microblog");

            entity.HasIndex(e => e.Role, "uq_community_role_role").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Role)
                .HasMaxLength(50)
                .HasColumnName("role");
        });

        modelBuilder.Entity<CommunityUserRole>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("community_user_role_pkey");

            entity.ToTable("community_user_role", "Microblog");

            entity.HasIndex(e => new { e.CommunityId, e.RegularUserId }, "uq_cur_user_per_community").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CommunityId).HasColumnName("community_id");
            entity.Property(e => e.CommunityRoleId).HasColumnName("community_role_id");
            entity.Property(e => e.RegularUserId).HasColumnName("regular_user_id");

            entity.HasOne(d => d.Community).WithMany(p => p.CommunityUserRoles)
                .HasForeignKey(d => d.CommunityId)
                .HasConstraintName("fk_cur_community");

            entity.HasOne(d => d.CommunityRole).WithMany(p => p.CommunityUserRoles)
                .HasForeignKey(d => d.CommunityRoleId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_cur_role");

            entity.HasOne(d => d.RegularUser).WithMany(p => p.CommunityUserRoles)
                .HasForeignKey(d => d.RegularUserId)
                .HasConstraintName("fk_cur_user");
        });

        modelBuilder.Entity<Follower>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("follower_pkey");

            entity.ToTable("follower", "Microblog");

            entity.HasIndex(e => new { e.RegularUserFollowerId, e.RegularUserFollowingId }, "uq_follower").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.RegularUserFollowerId).HasColumnName("regular_user_follower_id");
            entity.Property(e => e.RegularUserFollowingId).HasColumnName("regular_user_following_id");

            entity.HasOne(d => d.RegularUserFollower).WithMany(p => p.FollowerRegularUserFollowers)
                .HasForeignKey(d => d.RegularUserFollowerId)
                .HasConstraintName("fk_follower_follower");

            entity.HasOne(d => d.RegularUserFollowing).WithMany(p => p.FollowerRegularUserFollowings)
                .HasForeignKey(d => d.RegularUserFollowingId)
                .HasConstraintName("fk_follower_following");
        });

        modelBuilder.Entity<HashTag>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("hash_tag_pkey");

            entity.ToTable("hash_tag", "Microblog");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
        });

        modelBuilder.Entity<HashTagFollow>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("hash_tag_follow_pkey");

            entity.ToTable("hash_tag_follow", "Microblog");

            entity.HasIndex(e => new { e.RegularUserId, e.HashTagId }, "uq_hash_tag_follow").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.HashTagId).HasColumnName("hash_tag_id");
            entity.Property(e => e.RegularUserId).HasColumnName("regular_user_id");

            entity.HasOne(d => d.HashTag).WithMany(p => p.HashTagFollows)
                .HasForeignKey(d => d.HashTagId)
                .HasConstraintName("fk_hash_tag_follow_tag");

            entity.HasOne(d => d.RegularUser).WithMany(p => p.HashTagFollows)
                .HasForeignKey(d => d.RegularUserId)
                .HasConstraintName("fk_hash_tag_follow_user");
        });

        modelBuilder.Entity<Media>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("media_pkey");

            entity.ToTable("media", "Microblog");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Path)
                .HasMaxLength(500)
                .HasColumnName("path");
        });

        modelBuilder.Entity<Post>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("post_pkey");

            entity.ToTable("post", "Microblog");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CommunityId).HasColumnName("community_id");
            entity.Property(e => e.Content)
                .HasMaxLength(500)
                .HasColumnName("content");            
            entity.Property(e => e.DatePosted).HasColumnName("date_posted").HasColumnType("timestamp without time zone");
            entity.Property(e => e.ParentPostId).HasColumnName("parent_post_id");
            entity.Property(e => e.RegularUserId).HasColumnName("regular_user_id");

            entity.HasOne(d => d.Community).WithMany(p => p.Posts)
                .HasForeignKey(d => d.CommunityId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_post_community");

            entity.HasOne(d => d.ParentPost).WithMany(p => p.InverseParentPost)
                .HasForeignKey(d => d.ParentPostId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_post_parent");

            entity.HasOne(d => d.RegularUser).WithMany(p => p.Posts)
                .HasForeignKey(d => d.RegularUserId)
                .HasConstraintName("fk_post_user");
        });

        modelBuilder.Entity<PostFollow>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("post_follow_pkey");

            entity.ToTable("post_follow", "Microblog");

            entity.HasIndex(e => new { e.RegularUserId, e.PostId }, "uq_post_follow").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.PostId).HasColumnName("post_id");
            entity.Property(e => e.RegularUserId).HasColumnName("regular_user_id");

            entity.HasOne(d => d.Post).WithMany(p => p.PostFollows)
                .HasForeignKey(d => d.PostId)
                .HasConstraintName("fk_post_follow_post");

            entity.HasOne(d => d.RegularUser).WithMany(p => p.PostFollows)
                .HasForeignKey(d => d.RegularUserId)
                .HasConstraintName("fk_post_follow_user");
        });

        modelBuilder.Entity<PostHashTag>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("post_hash_tag_pkey");

            entity.ToTable("post_hash_tag", "Microblog");

            entity.HasIndex(e => new { e.PostId, e.HashTagId }, "uq_post_hash_tag").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.HashTagId).HasColumnName("hash_tag_id");
            entity.Property(e => e.PostId).HasColumnName("post_id");

            entity.HasOne(d => d.HashTag).WithMany(p => p.PostHashTags)
                .HasForeignKey(d => d.HashTagId)
                .HasConstraintName("fk_post_hash_tag_tag");

            entity.HasOne(d => d.Post).WithMany(p => p.PostHashTags)
                .HasForeignKey(d => d.PostId)
                .HasConstraintName("fk_post_hash_tag_post");
        });

        modelBuilder.Entity<PostLike>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("post_like_pkey");

            entity.ToTable("post_like", "Microblog");

            entity.HasIndex(e => new { e.RegularUserId, e.PostId }, "uq_post_like").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.PostId).HasColumnName("post_id");
            entity.Property(e => e.RegularUserId).HasColumnName("regular_user_id");

            entity.HasOne(d => d.Post).WithMany(p => p.PostLikes)
                .HasForeignKey(d => d.PostId)
                .HasConstraintName("fk_post_like_post");

            entity.HasOne(d => d.RegularUser).WithMany(p => p.PostLikes)
                .HasForeignKey(d => d.RegularUserId)
                .HasConstraintName("fk_post_like_user");
        });

        modelBuilder.Entity<PostMedia>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("post_media_pkey");

            entity.ToTable("post_media", "Microblog");

            entity.HasIndex(e => new { e.PostId, e.MediaId }, "uq_post_media").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.MediaId).HasColumnName("media_id");
            entity.Property(e => e.PostId).HasColumnName("post_id");

            entity.HasOne(d => d.Media).WithMany(p => p.PostMedia)
                .HasForeignKey(d => d.MediaId)
                .HasConstraintName("fk_post_media_media");

            entity.HasOne(d => d.Post).WithMany(p => p.PostMedia)
                .HasForeignKey(d => d.PostId)
                .HasConstraintName("fk_post_media_post");
        });

        modelBuilder.Entity<PostMissuse>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("post_missuse_pkey");

            entity.ToTable("post_missuse", "Microblog");

            entity.HasIndex(e => new { e.RegularUserId, e.PostId, e.MissuseName }, "uq_post_missuse").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.MissuseDescription)
                .HasMaxLength(200)
                .HasColumnName("missuse_description");
            entity.Property(e => e.MissuseName)
                .HasMaxLength(50)
                .HasColumnName("missuse_name");
            entity.Property(e => e.PostId).HasColumnName("post_id");
            entity.Property(e => e.RegularUserId).HasColumnName("regular_user_id");

            entity.HasOne(d => d.Post).WithMany(p => p.PostMissuses)
                .HasForeignKey(d => d.PostId)
                .HasConstraintName("fk_post_missuse_post");

            entity.HasOne(d => d.RegularUser).WithMany(p => p.PostMissuses)
                .HasForeignKey(d => d.RegularUserId)
                .HasConstraintName("fk_post_missuse_user");
        });

        modelBuilder.Entity<RegularUser>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("regular_user_pkey");

            entity.ToTable("regular_user", "Microblog");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Email)
                .HasMaxLength(50)
                .HasColumnName("email");
            entity.Property(e => e.FirstName)
                .HasMaxLength(50)
                .HasColumnName("first_name");
            entity.Property(e => e.LastName)
                .HasMaxLength(50)
                .HasColumnName("last_name");
            entity.Property(e => e.Password)
                .HasMaxLength(50)
                .HasColumnName("password");
            entity.Property(e => e.Username)
                .HasMaxLength(50)
                .HasColumnName("username");
            // Existing rows get the "User" role when the column is added.
            entity.Property(e => e.UserRoleID)
                .HasColumnName("user_role_id")
                .HasDefaultValue(UserRole.UserRoleId);

            entity.HasOne(d => d.UserRole).WithMany(p => p.RegularUsers)
                .HasForeignKey(d => d.UserRoleID)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_regular_user_user_role");
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("user_role_pkey");

            entity.ToTable("user_role", "Microblog");

            entity.HasIndex(e => e.Role, "uq_user_role_role").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Role)
                .HasMaxLength(50)
                .HasColumnName("role");

            entity.HasData(
                new UserRole { Id = UserRole.UserRoleId, Role = "User" },
                new UserRole { Id = UserRole.AdminRoleId, Role = "Admin" });
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
