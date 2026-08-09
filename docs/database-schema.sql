-- Gym Membership Management System - Database Schema
-- Generated for SQL Server (LocalDB / Express / Full)
-- Matches the current EF Core model (InitialCreate + FixPendingPaymentsToCompleted migrations).
-- Run this script to create the database schema manually if needed.

USE [GMMSDb];
GO

/****** Table: Tbl_AuditLog ******/
CREATE TABLE [dbo].[Tbl_AuditLog](
	[AuditId] [bigint] IDENTITY(1,1) NOT NULL,
	[TableName] [nvarchar](100) NOT NULL,
	[RecordId] [nvarchar](50) NOT NULL,
	[Action] [nvarchar](50) NOT NULL,
	[UserId] [int] NOT NULL,
	[OldValue] [nvarchar](max) NULL,
	[NewValue] [nvarchar](max) NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
	CONSTRAINT [PK_TblAuditLog] PRIMARY KEY CLUSTERED ([AuditId] ASC)
) ON [PRIMARY];
GO
ALTER TABLE [dbo].[Tbl_AuditLog] ADD DEFAULT (getdate()) FOR [CreatedAt];
GO

/****** Table: Tbl_Member ******/
CREATE TABLE [dbo].[Tbl_Member](
	[MemberId] [int] IDENTITY(1,1) NOT NULL,
	[MemberCode] [nvarchar](50) NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
	[IsDeleted] [bit] NOT NULL,
	[CreatedBy] [int] NOT NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
	[UpdatedBy] [int] NULL,
	[UpdatedAt] [datetime2](7) NULL,
	CONSTRAINT [PK__Tbl_Memb__0CF04B1805BB98F3] PRIMARY KEY CLUSTERED ([MemberId] ASC)
) ON [PRIMARY];
GO
ALTER TABLE [dbo].[Tbl_Member] ADD CONSTRAINT [UQ__Tbl_Memb__84CA637700FA42E5] UNIQUE NONCLUSTERED ([MemberCode] ASC);
GO
ALTER TABLE [dbo].[Tbl_Member] ADD DEFAULT ((0)) FOR [IsDeleted];
GO
ALTER TABLE [dbo].[Tbl_Member] ADD DEFAULT (getdate()) FOR [CreatedAt];
GO

/****** Table: Tbl_MembershipPlan ******/
CREATE TABLE [dbo].[Tbl_MembershipPlan](
	[MembershipPlanId] [int] IDENTITY(1,1) NOT NULL,
	[PlanCode] [nvarchar](50) NOT NULL,
	[PlanName] [nvarchar](100) NOT NULL,
	[Price] [decimal](18, 2) NOT NULL,
	[DurationDays] [int] NOT NULL,
	[Description] [nvarchar](500) NULL,
	[IsActive] [bit] NOT NULL,
	[IsDeleted] [bit] NOT NULL,
	[CreatedBy] [int] NOT NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
	[UpdatedBy] [int] NULL,
	[UpdatedAt] [datetime2](7) NULL,
	CONSTRAINT [PK__Tbl_Memb__8E444BB63E2A6ABC] PRIMARY KEY CLUSTERED ([MembershipPlanId] ASC)
) ON [PRIMARY];
GO
ALTER TABLE [dbo].[Tbl_MembershipPlan] ADD DEFAULT ((1)) FOR [IsActive];
GO
ALTER TABLE [dbo].[Tbl_MembershipPlan] ADD DEFAULT ((0)) FOR [IsDeleted];
GO
ALTER TABLE [dbo].[Tbl_MembershipPlan] ADD DEFAULT (getdate()) FOR [CreatedAt];
GO

/****** Table: Tbl_PaymentMethod ******/
CREATE TABLE [dbo].[Tbl_PaymentMethod](
	[PaymentMethodId] [int] IDENTITY(1,1) NOT NULL,
	[PaymentMethodCode] [nvarchar](50) NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
	[IsActive] [bit] NOT NULL,
	[IsDeleted] [bit] NOT NULL,
	[CreatedBy] [int] NOT NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
	[UpdatedBy] [int] NULL,
	[UpdatedAt] [datetime2](7) NULL,
	CONSTRAINT [PK__Tbl_Paym__DC31C1D3D827C608] PRIMARY KEY CLUSTERED ([PaymentMethodId] ASC)
) ON [PRIMARY];
GO
ALTER TABLE [dbo].[Tbl_PaymentMethod] ADD DEFAULT ((1)) FOR [IsActive];
GO
ALTER TABLE [dbo].[Tbl_PaymentMethod] ADD DEFAULT ((0)) FOR [IsDeleted];
GO
ALTER TABLE [dbo].[Tbl_PaymentMethod] ADD DEFAULT (getdate()) FOR [CreatedAt];
GO

/****** Table: Tbl_User ******/
CREATE TABLE [dbo].[Tbl_User](
	[UserId] [int] IDENTITY(1,1) NOT NULL,
	[UserName] [nvarchar](100) NOT NULL,
	[PasswordHash] [nvarchar](256) NOT NULL,
	[Role] [nvarchar](50) NOT NULL,
	[IsActive] [bit] NOT NULL,
	[MustChangePassword] [bit] NOT NULL,
	[IsDeleted] [bit] NOT NULL,
	[CreatedBy] [int] NOT NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
	[UpdatedBy] [int] NULL,
	[UpdatedAt] [datetime2](7) NULL,
	CONSTRAINT [PK_TblUser] PRIMARY KEY CLUSTERED ([UserId] ASC)
) ON [PRIMARY];
GO
ALTER TABLE [dbo].[Tbl_User] ADD CONSTRAINT [UQ_TblUser_UserName] UNIQUE NONCLUSTERED ([UserName] ASC);
GO
ALTER TABLE [dbo].[Tbl_User] ADD DEFAULT ((1)) FOR [IsActive];
GO
ALTER TABLE [dbo].[Tbl_User] ADD DEFAULT ((1)) FOR [MustChangePassword];
GO
ALTER TABLE [dbo].[Tbl_User] ADD DEFAULT ((0)) FOR [IsDeleted];
GO
ALTER TABLE [dbo].[Tbl_User] ADD DEFAULT (getdate()) FOR [CreatedAt];
GO

/****** Table: Tbl_Membership ******/
CREATE TABLE [dbo].[Tbl_Membership](
	[MembershipId] [int] IDENTITY(1,1) NOT NULL,
	[MemberId] [int] NOT NULL,
	[MembershipPlanId] [int] NOT NULL,
	[StartDate] [date] NOT NULL,
	[EndDate] [date] NOT NULL,
	[Status] [nvarchar](20) NOT NULL,
	[IsDeleted] [bit] NOT NULL,
	[CreatedBy] [int] NOT NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
	[UpdatedBy] [int] NULL,
	[UpdatedAt] [datetime2](7) NULL,
	CONSTRAINT [PK__Tbl_Memb__92A786791A0BD763] PRIMARY KEY CLUSTERED ([MembershipId] ASC)
) ON [PRIMARY];
GO
ALTER TABLE [dbo].[Tbl_Membership] ADD DEFAULT ((0)) FOR [IsDeleted];
GO
ALTER TABLE [dbo].[Tbl_Membership] ADD DEFAULT (getdate()) FOR [CreatedAt];
GO
ALTER TABLE [dbo].[Tbl_Membership] WITH CHECK ADD CONSTRAINT [FK_Membership_Member] FOREIGN KEY([MemberId])
REFERENCES [dbo].[Tbl_Member] ([MemberId]);
GO
ALTER TABLE [dbo].[Tbl_Membership] CHECK CONSTRAINT [FK_Membership_Member];
GO
ALTER TABLE [dbo].[Tbl_Membership] WITH CHECK ADD CONSTRAINT [FK_Membership_MembershipPlan] FOREIGN KEY([MembershipPlanId])
REFERENCES [dbo].[Tbl_MembershipPlan] ([MembershipPlanId]);
GO
ALTER TABLE [dbo].[Tbl_Membership] CHECK CONSTRAINT [FK_Membership_MembershipPlan];
GO
CREATE NONCLUSTERED INDEX [IX_Tbl_Membership_MemberId] ON [dbo].[Tbl_Membership]([MemberId] ASC);
GO
CREATE NONCLUSTERED INDEX [IX_Tbl_Membership_MembershipPlanId] ON [dbo].[Tbl_Membership]([MembershipPlanId] ASC);
GO

/****** Table: Tbl_UserSession ******/
CREATE TABLE [dbo].[Tbl_UserSession](
	[UserSessionId] [int] IDENTITY(1,1) NOT NULL,
	[SessionId] [uniqueidentifier] NOT NULL,
	[UserId] [int] NOT NULL,
	[RefreshTokenHash] [nvarchar](max) NOT NULL,
	[LoginTime] [datetime2](7) NOT NULL,
	[AccessTokenExpiresAt] [datetime2](7) NOT NULL,
	[RefreshTokenExpiresAt] [datetime2](7) NOT NULL,
	[RevokedAt] [datetime2](7) NULL,
	[IsExpired] [bit] NOT NULL,
	CONSTRAINT [PK_TblUserSession] PRIMARY KEY CLUSTERED ([UserSessionId] ASC)
) ON [PRIMARY];
GO
ALTER TABLE [dbo].[Tbl_UserSession] ADD DEFAULT (newid()) FOR [SessionId];
GO
ALTER TABLE [dbo].[Tbl_UserSession] ADD DEFAULT (getdate()) FOR [LoginTime];
GO
ALTER TABLE [dbo].[Tbl_UserSession] WITH CHECK ADD CONSTRAINT [FK_Tbl_UserSession_Tbl_User_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[Tbl_User] ([UserId])
ON DELETE CASCADE;
GO
ALTER TABLE [dbo].[Tbl_UserSession] CHECK CONSTRAINT [FK_Tbl_UserSession_Tbl_User_UserId];
GO
CREATE NONCLUSTERED INDEX [IX_Tbl_UserSession_UserId] ON [dbo].[Tbl_UserSession]([UserId] ASC);
GO

/****** Table: Tbl_Payment ******/
CREATE TABLE [dbo].[Tbl_Payment](
	[PaymentId] [int] IDENTITY(1,1) NOT NULL,
	[MembershipId] [int] NOT NULL,
	[PaymentMethodId] [int] NOT NULL,
	[Amount] [decimal](18, 2) NOT NULL,
	[SSPath] [nvarchar](500) NULL,
	[Status] [nvarchar](20) NOT NULL,
	[IsDeleted] [bit] NOT NULL,
	[CreatedBy] [int] NOT NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
	[UpdatedBy] [int] NULL,
	[UpdatedAt] [datetime2](7) NULL,
	CONSTRAINT [PK__Tbl_Paym__9B556A38D39F22DB] PRIMARY KEY CLUSTERED ([PaymentId] ASC)
) ON [PRIMARY];
GO
ALTER TABLE [dbo].[Tbl_Payment] ADD DEFAULT ((0)) FOR [IsDeleted];
GO
ALTER TABLE [dbo].[Tbl_Payment] ADD DEFAULT (getdate()) FOR [CreatedAt];
GO
ALTER TABLE [dbo].[Tbl_Payment] WITH CHECK ADD CONSTRAINT [FK_Payment_Membership] FOREIGN KEY([MembershipId])
REFERENCES [dbo].[Tbl_Membership] ([MembershipId]);
GO
ALTER TABLE [dbo].[Tbl_Payment] CHECK CONSTRAINT [FK_Payment_Membership];
GO
ALTER TABLE [dbo].[Tbl_Payment] WITH CHECK ADD CONSTRAINT [FK_Payment_PaymentMethod] FOREIGN KEY([PaymentMethodId])
REFERENCES [dbo].[Tbl_PaymentMethod] ([PaymentMethodId]);
GO
ALTER TABLE [dbo].[Tbl_Payment] CHECK CONSTRAINT [FK_Payment_PaymentMethod];
GO
CREATE NONCLUSTERED INDEX [IX_Tbl_Payment_MembershipId] ON [dbo].[Tbl_Payment]([MembershipId] ASC);
GO
CREATE NONCLUSTERED INDEX [IX_Tbl_Payment_PaymentMethodId] ON [dbo].[Tbl_Payment]([PaymentMethodId] ASC);
GO

/****** Seed users (bcrypt hashes; both must change password on first login) ******/
INSERT INTO [dbo].[Tbl_User]
	([UserId], [UserName], [PasswordHash], [Role], [IsActive], [MustChangePassword], [IsDeleted], [CreatedBy], [CreatedAt], [UpdatedBy], [UpdatedAt])
VALUES
	(1, N'owner', N'$2a$11$51n3uUKJ0zfp8Suf/AH2wulPNfkwy4CqEslohD8VpYIwA5gTaOXKG', N'Owner', 1, 1, 0, 1, '2026-01-01 00:00:00.0000000', NULL, NULL),
	(2, N'admin', N'$2a$11$GJjdLlC9Kb9d4LGvnYVHO.IiBoNPUTRCDS4.TR92T7bHxTveFHtkq', N'Admin', 1, 1, 0, 1, '2026-01-01 00:00:00.0000000', NULL, NULL);
GO
