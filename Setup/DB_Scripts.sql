-- Create AccountExamDB
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'AccountExamDB')
BEGIN
    CREATE DATABASE [AccountExamDB]
END
GO

-- Create FeedbackExamDB
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'FeedbackExamDB')
BEGIN
    CREATE DATABASE [FeedbackExamDB]
END
GO

-- Create ManagementExamDB
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'ManagementExamDB')
BEGIN
    CREATE DATABASE [ManagementExamDB]
END
GO

-- Create OrderExamDB
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'OrderExamDB')
BEGIN
    CREATE DATABASE [OrderExamDB]
END
GO

-- Create SearchExamDB
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'SearchExamDB')
BEGIN
    CREATE DATABASE [SearchExamDB]
END
GO
USE [AccountExamDB]
GO
/****** Object:  Table [dbo].[Account]    Script Date: 11-12-2024 12:53:10 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Account]') AND type in (N'U'))
DROP TABLE [dbo].[Account]
GO
/****** Object:  User [ExamDBUser]    Script Date: 11-12-2024 12:53:10 ******/
DROP USER [ExamDBUser]
GO
/****** Object:  User [ExamDBUser]    Script Date: 11-12-2024 12:53:10 ******/
CREATE USER [ExamDBUser] FOR LOGIN [ExamDBUser] WITH DEFAULT_SCHEMA=[dbo]
GO
ALTER ROLE [db_owner] ADD MEMBER [ExamDBUser]
GO
/****** Object:  Table [dbo].[Account]    Script Date: 11-12-2024 12:53:10 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Account](
	[AccountID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](100) NOT NULL,
	[Email] [varchar](255) NOT NULL,
	[PhoneNumber] [varchar](15) NOT NULL,
	[Password] [varchar](max) NOT NULL,
	[Language] [varchar](50) NOT NULL,
	[CreatedAt] [datetime] NOT NULL,
	[Address] [varchar](255) NULL,
	[PaymentInfo] [varchar](255) NULL,
	[Status] [varchar](50) NULL,
	[RestaurantSearchID] [int] NULL,
	[AccountType] [int] NOT NULL,
	[StatusChanged] [datetime] NOT NULL,
 CONSTRAINT [PK_Account] PRIMARY KEY CLUSTERED 
(
	[AccountID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
USE [FeedbackExamDB]
GO
/****** Object:  Table [dbo].[Feedback]    Script Date: 11-12-2024 12:55:11 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Feedback]') AND type in (N'U'))
DROP TABLE [dbo].[Feedback]
GO
/****** Object:  User [ExamDBUser]    Script Date: 11-12-2024 12:55:11 ******/
DROP USER [ExamDBUser]
GO
/****** Object:  User [ExamDBUser]    Script Date: 11-12-2024 12:55:11 ******/
CREATE USER [ExamDBUser] FOR LOGIN [ExamDBUser] WITH DEFAULT_SCHEMA=[dbo]
GO
ALTER ROLE [db_owner] ADD MEMBER [ExamDBUser]
GO
/****** Object:  Table [dbo].[Feedback]    Script Date: 11-12-2024 12:55:11 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Feedback](
	[FeedbackID] [int] IDENTITY(1,1) NOT NULL,
	[FoodRating] [float] NOT NULL,
	[DeliveryRating] [float] NOT NULL,
	[Comment] [varchar](1000) NULL,
	[RestaurantID] [int] NOT NULL,
	[DeliveryDriverID] [int] NOT NULL,
 CONSTRAINT [PK_Feedback] PRIMARY KEY CLUSTERED 
(
	[FeedbackID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
USE [ManagementExamDB]
GO
/****** Object:  Table [dbo].[Payment]    Script Date: 11-12-2024 12:59:27 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Payment]') AND type in (N'U'))
DROP TABLE [dbo].[Payment]
GO
/****** Object:  Table [dbo].[Fee]    Script Date: 11-12-2024 12:59:27 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Fee]') AND type in (N'U'))
DROP TABLE [dbo].[Fee]
GO
/****** Object:  User [ExamDBUser]    Script Date: 11-12-2024 12:59:27 ******/
DROP USER [ExamDBUser]
GO
/****** Object:  User [ExamDBUser]    Script Date: 11-12-2024 12:59:27 ******/
CREATE USER [ExamDBUser] FOR LOGIN [ExamDBUser] WITH DEFAULT_SCHEMA=[dbo]
GO
ALTER ROLE [db_owner] ADD MEMBER [ExamDBUser]
GO
/****** Object:  Table [dbo].[Fee]    Script Date: 11-12-2024 12:59:27 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Fee](
	[FeeID] [int] IDENTITY(1,1) NOT NULL,
	[Amount] [decimal](18, 2) NOT NULL,
	[RestaurantID] [int] NOT NULL,
	[OrderCount] [int] NOT NULL,
	[TotalOrderPrice] [decimal](18, 2) NOT NULL,
	[InvoiceDate] [datetime] NOT NULL,
	[DueDate] [datetime] NOT NULL,
	[PaidDate] [datetime] NOT NULL,
	[Status] [varchar](50) NOT NULL,
 CONSTRAINT [PK_Fee] PRIMARY KEY CLUSTERED 
(
	[FeeID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Payment]    Script Date: 11-12-2024 12:59:28 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Payment](
	[PaymentID] [int] IDENTITY(1,1) NOT NULL,
	[Amount] [decimal](18, 2) NOT NULL,
	[DeliveryDriverID] [int] NOT NULL,
	[PaycheckDate] [datetime] NOT NULL,
	[PaidDate] [datetime] NOT NULL,
	[TotalDeliveryPrice] [decimal](18, 2) NOT NULL,
	[Status] [varchar](50) NOT NULL,
	[DeliveryCount] [int] NOT NULL,
 CONSTRAINT [PK_Payment] PRIMARY KEY CLUSTERED 
(
	[PaymentID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
USE [OrderExamDB]
GO
ALTER TABLE [dbo].[OrderItem] DROP CONSTRAINT [FK_OrderItem_Order]
GO
/****** Object:  Table [dbo].[OrderItem]    Script Date: 11-12-2024 13:00:12 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[OrderItem]') AND type in (N'U'))
DROP TABLE [dbo].[OrderItem]
GO
/****** Object:  Table [dbo].[Order]    Script Date: 11-12-2024 13:00:12 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Order]') AND type in (N'U'))
DROP TABLE [dbo].[Order]
GO
/****** Object:  User [ExamDBUser]    Script Date: 11-12-2024 13:00:12 ******/
DROP USER [ExamDBUser]
GO
/****** Object:  User [ExamDBUser]    Script Date: 11-12-2024 13:00:12 ******/
CREATE USER [ExamDBUser] FOR LOGIN [ExamDBUser] WITH DEFAULT_SCHEMA=[dbo]
GO
ALTER ROLE [db_owner] ADD MEMBER [ExamDBUser]
GO
/****** Object:  Table [dbo].[Order]    Script Date: 11-12-2024 13:00:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Order](
	[OrderID] [int] IDENTITY(1,1) NOT NULL,
	[CustomerID] [int] NOT NULL,
	[TotalPrice] [decimal](18, 2) NOT NULL,
	[Status] [varchar](50) NOT NULL,
	[RestaurantID] [int] NOT NULL,
	[DriverID] [int] NULL,
	[OrderPlaced] [datetime] NOT NULL,
	[OrderDelivered] [datetime] NOT NULL,
 CONSTRAINT [PK_Order] PRIMARY KEY CLUSTERED 
(
	[OrderID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[OrderItem]    Script Date: 11-12-2024 13:00:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[OrderItem](
	[OrderItemID] [int] IDENTITY(1,1) NOT NULL,
	[OrderID] [int] NOT NULL,
	[MenuItemID] [int] NOT NULL,
	[Quantity] [int] NOT NULL,
	[TotalPrice] [decimal](18, 2) NOT NULL,
 CONSTRAINT [PK_OrderItem] PRIMARY KEY CLUSTERED 
(
	[OrderItemID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[OrderItem]  WITH CHECK ADD  CONSTRAINT [FK_OrderItem_Order] FOREIGN KEY([OrderID])
REFERENCES [dbo].[Order] ([OrderID])
GO
ALTER TABLE [dbo].[OrderItem] CHECK CONSTRAINT [FK_OrderItem_Order]
GO
USE [SearchExamDB]
GO
ALTER TABLE [dbo].[MenuItem] DROP CONSTRAINT [FK_MenuItem_Restaurant]
GO
ALTER TABLE [dbo].[Categories] DROP CONSTRAINT [FK_Categories_Restaurant]
GO
/****** Object:  Table [dbo].[Restaurant]    Script Date: 11-12-2024 13:00:57 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Restaurant]') AND type in (N'U'))
DROP TABLE [dbo].[Restaurant]
GO
/****** Object:  Table [dbo].[MenuItem]    Script Date: 11-12-2024 13:00:57 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MenuItem]') AND type in (N'U'))
DROP TABLE [dbo].[MenuItem]
GO
/****** Object:  Table [dbo].[Categories]    Script Date: 11-12-2024 13:00:57 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Categories]') AND type in (N'U'))
DROP TABLE [dbo].[Categories]
GO
/****** Object:  User [ExamDBUser]    Script Date: 11-12-2024 13:00:57 ******/
DROP USER [ExamDBUser]
GO
/****** Object:  User [ExamDBUser]    Script Date: 11-12-2024 13:00:57 ******/
CREATE USER [ExamDBUser] FOR LOGIN [ExamDBUser] WITH DEFAULT_SCHEMA=[dbo]
GO
ALTER ROLE [db_owner] ADD MEMBER [ExamDBUser]
GO
/****** Object:  Table [dbo].[Categories]    Script Date: 11-12-2024 13:00:57 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Categories](
	[CategoryID] [int] IDENTITY(1,1) NOT NULL,
	[RestaurantID] [int] NOT NULL,
	[Category] [varchar](255) NOT NULL,
 CONSTRAINT [PK_Categories] PRIMARY KEY CLUSTERED 
(
	[CategoryID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[MenuItem]    Script Date: 11-12-2024 13:00:57 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MenuItem](
	[MenuItemID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](50) NOT NULL,
	[Price] [decimal](18, 2) NOT NULL,
	[Description] [varchar](255) NULL,
	[RestaurantID] [int] NOT NULL,
 CONSTRAINT [PK_MenuItem] PRIMARY KEY CLUSTERED 
(
	[MenuItemID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Restaurant]    Script Date: 11-12-2024 13:00:57 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Restaurant](
	[RestaurantID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](50) NOT NULL,
	[OpeningHours] [varchar](255) NOT NULL,
	[Address] [varchar](50) NOT NULL,
 CONSTRAINT [PK_Restaurant] PRIMARY KEY CLUSTERED 
(
	[RestaurantID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[Categories]  WITH CHECK ADD  CONSTRAINT [FK_Categories_Restaurant] FOREIGN KEY([RestaurantID])
REFERENCES [dbo].[Restaurant] ([RestaurantID])
GO
ALTER TABLE [dbo].[Categories] CHECK CONSTRAINT [FK_Categories_Restaurant]
GO
ALTER TABLE [dbo].[MenuItem]  WITH CHECK ADD  CONSTRAINT [FK_MenuItem_Restaurant] FOREIGN KEY([RestaurantID])
REFERENCES [dbo].[Restaurant] ([RestaurantID])
GO
ALTER TABLE [dbo].[MenuItem] CHECK CONSTRAINT [FK_MenuItem_Restaurant]
GO
