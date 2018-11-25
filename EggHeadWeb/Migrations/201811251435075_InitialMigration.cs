namespace EggHeadWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialMigration : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.AdminEmailTemplate",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AdminId = c.Long(nullable: false),
                        Name = c.String(nullable: false, maxLength: 50),
                        MailSubject = c.String(nullable: false, maxLength: 200),
                        MailBody = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Admin", t => t.AdminId)
                .Index(t => t.AdminId);
            
            CreateTable(
                "dbo.Admin",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        AreaId = c.Long(),
                        Username = c.String(nullable: false, maxLength: 50),
                        Password = c.String(nullable: false, maxLength: 200),
                        FirstName = c.String(maxLength: 50),
                        LastName = c.String(maxLength: 50),
                        PhoneNumber = c.String(maxLength: 50),
                        Address = c.String(maxLength: 200),
                        City = c.String(maxLength: 50),
                        State = c.String(maxLength: 50),
                        Zip = c.String(maxLength: 50, unicode: false),
                        Email = c.String(maxLength: 50),
                        EmailPassword = c.String(maxLength: 200),
                        IsSuperAdmin = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Area", t => t.AreaId)
                .Index(t => t.AreaId);
            
            CreateTable(
                "dbo.AdminFrontend",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AdminId = c.Long(nullable: false),
                        FrontendId = c.Int(),
                        MenuName = c.String(maxLength: 50),
                        Name = c.String(maxLength: 50),
                        IsActive = c.Boolean(nullable: false),
                        OverridePageContent = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Frontend", t => t.FrontendId)
                .ForeignKey("dbo.Admin", t => t.AdminId)
                .Index(t => t.AdminId)
                .Index(t => t.FrontendId);
            
            CreateTable(
                "dbo.Frontend",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        MenuName = c.String(nullable: false, maxLength: 50),
                        Name = c.String(nullable: false, maxLength: 50),
                        IsActive = c.Boolean(nullable: false),
                        PageContent = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.AdminPaymentInfo",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        AdminId = c.Long(nullable: false),
                        APILoginID = c.String(nullable: false, maxLength: 100),
                        TransactionKey = c.String(nullable: false, maxLength: 100),
                        MD5HashPhrase = c.String(maxLength: 100),
                        LastUpdateDate = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Admin", t => t.AdminId)
                .Index(t => t.AdminId);
            
            CreateTable(
                "dbo.AdminTask",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        AdminId = c.Long(nullable: false),
                        Name = c.String(nullable: false, maxLength: 150),
                        Priority = c.Byte(nullable: false),
                        Status = c.Byte(nullable: false),
                        DueDate = c.DateTime(),
                        Notes = c.String(maxLength: 200),
                        CreateDate = c.DateTime(nullable: false, storeType: "date"),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Admin", t => t.AdminId)
                .Index(t => t.AdminId);
            
            CreateTable(
                "dbo.Area",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 50),
                        DisplayName = c.String(nullable: false, maxLength: 50),
                        Description = c.String(maxLength: 200),
                        State = c.String(maxLength: 50),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Birthday",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        ParentName = c.String(nullable: false, maxLength: 50),
                        ContactNumber = c.String(nullable: false, maxLength: 50, unicode: false),
                        Email = c.String(nullable: false, maxLength: 50),
                        Address = c.String(nullable: false, maxLength: 200),
                        ChildName = c.String(nullable: false, maxLength: 50),
                        Age = c.Int(nullable: false),
                        PartyDate = c.DateTime(nullable: false),
                        PartyTime = c.Time(nullable: false, precision: 7),
                        Notes = c.String(nullable: false),
                        AreaId = c.Long(nullable: false),
                        InputDate = c.DateTime(),
                        InstructorId = c.Long(nullable: false),
                        AssistantId = c.Long(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Area", t => t.AreaId)
                .Index(t => t.AreaId);
            
            CreateTable(
                "dbo.Assign",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        Date = c.DateTime(nullable: false),
                        InstructorId = c.Long(nullable: false),
                        AssistantId = c.Long(),
                        ClassId = c.Long(),
                        CampId = c.Long(),
                        BirthdayId = c.Long(),
                        WorkshopId = c.Long(),
                        NDate = c.DateTime(),
                        DateTimeStart = c.DateTime(nullable: false),
                        DateTimeEnd = c.DateTime(nullable: false),
                        Name = c.String(),
                        Camp_Id = c.Long(),
                        Camp_Id1 = c.Long(),
                        Class_Id = c.Long(),
                        Class_Id1 = c.Long(),
                        Workshop_Id = c.Long(),
                        Workshop_Id1 = c.Long(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Birthday", t => t.BirthdayId)
                .ForeignKey("dbo.Camp", t => t.Camp_Id)
                .ForeignKey("dbo.Camp", t => t.Camp_Id1)
                .ForeignKey("dbo.Class", t => t.Class_Id)
                .ForeignKey("dbo.Class", t => t.Class_Id1)
                .ForeignKey("dbo.Workshop", t => t.Workshop_Id)
                .ForeignKey("dbo.Workshop", t => t.Workshop_Id1)
                .ForeignKey("dbo.Instructor", t => t.InstructorId)
                .ForeignKey("dbo.Instructor", t => t.AssistantId)
                .ForeignKey("dbo.Camp", t => t.CampId)
                .ForeignKey("dbo.Class", t => t.ClassId)
                .ForeignKey("dbo.Workshop", t => t.WorkshopId)
                .Index(t => t.InstructorId)
                .Index(t => t.AssistantId)
                .Index(t => t.ClassId)
                .Index(t => t.CampId)
                .Index(t => t.BirthdayId)
                .Index(t => t.WorkshopId)
                .Index(t => t.Camp_Id)
                .Index(t => t.Camp_Id1)
                .Index(t => t.Class_Id)
                .Index(t => t.Class_Id1)
                .Index(t => t.Workshop_Id)
                .Index(t => t.Workshop_Id1);
            
            CreateTable(
                "dbo.Camp",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        LocationId = c.Long(nullable: false),
                        Name = c.String(nullable: false, maxLength: 200),
                        OnlineName = c.String(nullable: false, maxLength: 200),
                        GradeGroupId = c.Long(nullable: false),
                        TimeStart = c.Time(nullable: false, precision: 7),
                        TimeEnd = c.Time(nullable: false, precision: 7),
                        InstructorId = c.Long(nullable: false),
                        AssistantId = c.Long(),
                        CanRegistOnline = c.Boolean(nullable: false),
                        IsOpen = c.Boolean(nullable: false),
                        DisplayUntil = c.DateTime(),
                        MaxEnroll = c.Int(nullable: false),
                        Cost = c.Decimal(nullable: false, precision: 18, scale: 0),
                        OnlineDescription = c.String(),
                        Notes = c.String(),
                        Enrolled = c.Int(),
                        InputDate = c.DateTime(storeType: "date"),
                        NCost = c.Decimal(precision: 18, scale: 2),
                        NDisplayUntil = c.DateTime(),
                        NMaxEnroll = c.Int(),
                        Dates = c.String(),
                        DateListText = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.GradeGroup", t => t.GradeGroupId)
                .ForeignKey("dbo.Location", t => t.LocationId)
                .ForeignKey("dbo.Instructor", t => t.InstructorId)
                .ForeignKey("dbo.Instructor", t => t.AssistantId)
                .Index(t => t.LocationId)
                .Index(t => t.GradeGroupId)
                .Index(t => t.InstructorId)
                .Index(t => t.AssistantId);
            
            CreateTable(
                "dbo.Booking",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        CampId = c.Long(),
                        ClassId = c.Long(),
                        WorkshopId = c.Long(),
                        BirthdayId = c.Long(),
                        StudentId = c.Long(nullable: false),
                        BookDate = c.DateTime(nullable: false),
                        CouponId = c.Long(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Birthday", t => t.BirthdayId)
                .ForeignKey("dbo.Camp", t => t.CampId)
                .ForeignKey("dbo.Class", t => t.ClassId)
                .ForeignKey("dbo.Student", t => t.StudentId)
                .ForeignKey("dbo.Workshop", t => t.WorkshopId)
                .ForeignKey("dbo.Coupon", t => t.CouponId)
                .Index(t => t.CampId)
                .Index(t => t.ClassId)
                .Index(t => t.WorkshopId)
                .Index(t => t.BirthdayId)
                .Index(t => t.StudentId)
                .Index(t => t.CouponId);
            
            CreateTable(
                "dbo.Class",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        LocationId = c.Long(nullable: false),
                        Name = c.String(nullable: false, maxLength: 200),
                        OnlineName = c.String(nullable: false, maxLength: 200),
                        GradeGroupId = c.Long(nullable: false),
                        TimeStart = c.Time(nullable: false, precision: 7),
                        TimeEnd = c.Time(nullable: false, precision: 7),
                        InstructorId = c.Long(nullable: false),
                        AssistantId = c.Long(),
                        CanRegistOnline = c.Boolean(nullable: false),
                        IsOpen = c.Boolean(nullable: false),
                        DisplayUntil = c.DateTime(),
                        MaxEnroll = c.Int(nullable: false),
                        Cost = c.Decimal(nullable: false, precision: 18, scale: 0),
                        OnlineDescription = c.String(),
                        Notes = c.String(),
                        Enrolled = c.Int(),
                        InputDate = c.DateTime(storeType: "date"),
                        DateListText = c.String(),
                        NCost = c.Decimal(precision: 18, scale: 2),
                        NDisplayUntil = c.DateTime(),
                        NMaxEnroll = c.Int(),
                        Dates = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.GradeGroup", t => t.GradeGroupId)
                .ForeignKey("dbo.Location", t => t.LocationId)
                .ForeignKey("dbo.Instructor", t => t.InstructorId)
                .ForeignKey("dbo.Instructor", t => t.AssistantId)
                .Index(t => t.LocationId)
                .Index(t => t.GradeGroupId)
                .Index(t => t.InstructorId)
                .Index(t => t.AssistantId);
            
            CreateTable(
                "dbo.GradeGroup",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        Name = c.String(maxLength: 50),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Grade",
                c => new
                    {
                        Id = c.Byte(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 50),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Student",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        ParentId = c.Long(nullable: false),
                        FirstName = c.String(nullable: false, maxLength: 50),
                        LastName = c.String(nullable: false, maxLength: 50),
                        GradeId = c.Byte(nullable: false),
                        Gender = c.String(nullable: false, maxLength: 1, fixedLength: true, unicode: false),
                        BirthDate = c.DateTime(nullable: false),
                        Notes = c.String(maxLength: 4000),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Parent", t => t.ParentId)
                .ForeignKey("dbo.Grade", t => t.GradeId)
                .Index(t => t.ParentId)
                .Index(t => t.GradeId);
            
            CreateTable(
                "dbo.Parent",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        LocationId = c.Long(nullable: false),
                        Email = c.String(nullable: false, maxLength: 50),
                        Password = c.String(nullable: false, unicode: false, storeType: "text"),
                        FirstName = c.String(nullable: false, maxLength: 50),
                        LastName = c.String(nullable: false, maxLength: 50),
                        PhoneNumer = c.String(nullable: false, maxLength: 50, unicode: false),
                        Address = c.String(nullable: false, maxLength: 200),
                        City = c.String(nullable: false, maxLength: 50),
                        State = c.String(nullable: false, maxLength: 50),
                        Zip = c.String(nullable: false, maxLength: 50, unicode: false),
                        LastLoginDateTime = c.DateTime(),
                        CreatedBy = c.Long(),
                        CreatedDate = c.DateTime(),
                        Note = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Location", t => t.LocationId)
                .Index(t => t.LocationId);
            
            CreateTable(
                "dbo.Location",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        AreaId = c.Long(nullable: false),
                        Name = c.String(nullable: false, maxLength: 200),
                        DisplayName = c.String(nullable: false, maxLength: 200),
                        PhoneNumber = c.String(maxLength: 50, unicode: false),
                        Address = c.String(maxLength: 200),
                        Email = c.String(maxLength: 100),
                        ContactPerson = c.String(maxLength: 100),
                        IsActive = c.Boolean(nullable: false),
                        CanRegistOnline = c.Boolean(nullable: false),
                        Note = c.String(maxLength: 4000),
                        UpdatedBy = c.Long(nullable: false),
                        UpdatedDate = c.DateTime(nullable: false, storeType: "date"),
                        City = c.String(maxLength: 50),
                        State = c.String(maxLength: 50),
                        Zip = c.String(maxLength: 50, unicode: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Area", t => t.AreaId)
                .Index(t => t.AreaId);
            
            CreateTable(
                "dbo.Workshop",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        LocationId = c.Long(nullable: false),
                        Name = c.String(nullable: false, maxLength: 200),
                        GradeGroupId = c.Long(nullable: false),
                        TimeStart = c.Time(nullable: false, precision: 7),
                        TimeEnd = c.Time(nullable: false, precision: 7),
                        InstructorId = c.Long(nullable: false),
                        AssistantId = c.Long(),
                        Cost = c.Decimal(nullable: false, precision: 18, scale: 0),
                        Notes = c.String(),
                        InputDate = c.DateTime(),
                        NCost = c.Decimal(precision: 18, scale: 2),
                        Dates = c.String(),
                        DateListText = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Instructor", t => t.InstructorId)
                .ForeignKey("dbo.Instructor", t => t.AssistantId)
                .ForeignKey("dbo.Location", t => t.LocationId)
                .ForeignKey("dbo.GradeGroup", t => t.GradeGroupId)
                .Index(t => t.LocationId)
                .Index(t => t.GradeGroupId)
                .Index(t => t.InstructorId)
                .Index(t => t.AssistantId);
            
            CreateTable(
                "dbo.Instructor",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        AreaId = c.Long(nullable: false),
                        FirstName = c.String(nullable: false, maxLength: 50),
                        LastName = c.String(nullable: false, maxLength: 50),
                        PhoneNumber = c.String(maxLength: 50, unicode: false),
                        Pay = c.String(maxLength: 100),
                        Address = c.String(maxLength: 200),
                        City = c.String(maxLength: 50),
                        State = c.String(maxLength: 50),
                        Zip = c.String(maxLength: 50, unicode: false),
                        Email = c.String(maxLength: 50),
                        Note = c.String(maxLength: 4000),
                        IsActive = c.Boolean(nullable: false),
                        CreatedBy = c.Long(nullable: false),
                        CreatedDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Area", t => t.AreaId)
                .Index(t => t.AreaId);
            
            CreateTable(
                "dbo.Coupon",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        AdminId = c.Long(),
                        Code = c.String(nullable: false, maxLength: 20, unicode: false),
                        Description = c.String(nullable: false, maxLength: 500),
                        Type = c.Byte(nullable: false),
                        DiscountAmount = c.Decimal(precision: 18, scale: 0),
                        ExpDate = c.DateTime(nullable: false),
                        MaxAvailable = c.Int(nullable: false),
                        MaxUsesPerCustomer = c.Int(nullable: false),
                        LastUsedDate = c.DateTime(),
                        NExpDate = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Admin", t => t.AdminId)
                .Index(t => t.AdminId);
            
            CreateTable(
                "dbo.PrivateMessage",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        FromAdminId = c.Long(),
                        ToAdminId = c.Long(),
                        MessageSubject = c.String(nullable: false, maxLength: 100),
                        MessageContent = c.String(nullable: false, unicode: false, storeType: "text"),
                        SendDate = c.DateTime(nullable: false),
                        Unread = c.Boolean(nullable: false),
                        FromAdminName = c.String(maxLength: 50),
                        ToAdminName = c.String(maxLength: 50),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Admin", t => t.FromAdminId)
                .ForeignKey("dbo.Admin", t => t.ToAdminId)
                .Index(t => t.FromAdminId)
                .Index(t => t.ToAdminId);
            
            CreateTable(
                "dbo.APVariable",
                c => new
                    {
                        Name = c.String(nullable: false, maxLength: 50, unicode: false),
                        Value = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.Name);
            
            CreateTable(
                "dbo.BookingShortInfoTemp",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        StudentId = c.Long(),
                        classId = c.Long(),
                        ServiceType = c.String(maxLength: 50),
                        Name = c.String(maxLength: 250),
                        Cost = c.Decimal(precision: 18, scale: 10),
                        Dates = c.String(maxLength: 500),
                        CouponId = c.Long(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.EmailTemplate",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Type = c.Int(nullable: false),
                        Name = c.String(nullable: false, maxLength: 50),
                        MailSubject = c.String(nullable: false, maxLength: 200),
                        MailBody = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Payment",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        AdminId = c.Long(),
                        ParentId = c.Long(nullable: false),
                        Amount = c.Decimal(nullable: false, precision: 18, scale: 0),
                        AuthCode = c.String(maxLength: 50),
                        TransactionID = c.String(maxLength: 50),
                        ServiceName = c.String(maxLength: 100),
                        PaymentMessage = c.String(unicode: false, storeType: "text"),
                        PaymentDate = c.DateTime(),
                        Bill_FirstName = c.String(maxLength: 50),
                        Bill_LastName = c.String(maxLength: 50),
                        Bill_Address = c.String(maxLength: 100),
                        Bill_City = c.String(maxLength: 100),
                        Bill_Zip = c.String(maxLength: 50),
                        Bill_State = c.String(maxLength: 50),
                        Bill_Email = c.String(maxLength: 50),
                        AdminParentId = c.Long(),
                        AdminAmount = c.String(),
                        Description = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.GradeGroupDetail",
                c => new
                    {
                        GradeId = c.Byte(nullable: false),
                        GradeGroupId = c.Long(nullable: false),
                    })
                .PrimaryKey(t => new { t.GradeId, t.GradeGroupId })
                .ForeignKey("dbo.Grade", t => t.GradeId, cascadeDelete: true)
                .ForeignKey("dbo.GradeGroup", t => t.GradeGroupId, cascadeDelete: true)
                .Index(t => t.GradeId)
                .Index(t => t.GradeGroupId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.PrivateMessage", "ToAdminId", "dbo.Admin");
            DropForeignKey("dbo.PrivateMessage", "FromAdminId", "dbo.Admin");
            DropForeignKey("dbo.Location", "AreaId", "dbo.Area");
            DropForeignKey("dbo.Instructor", "AreaId", "dbo.Area");
            DropForeignKey("dbo.Birthday", "AreaId", "dbo.Area");
            DropForeignKey("dbo.Assign", "WorkshopId", "dbo.Workshop");
            DropForeignKey("dbo.Assign", "ClassId", "dbo.Class");
            DropForeignKey("dbo.Assign", "CampId", "dbo.Camp");
            DropForeignKey("dbo.Booking", "CouponId", "dbo.Coupon");
            DropForeignKey("dbo.Coupon", "AdminId", "dbo.Admin");
            DropForeignKey("dbo.Workshop", "GradeGroupId", "dbo.GradeGroup");
            DropForeignKey("dbo.Student", "GradeId", "dbo.Grade");
            DropForeignKey("dbo.Student", "ParentId", "dbo.Parent");
            DropForeignKey("dbo.Workshop", "LocationId", "dbo.Location");
            DropForeignKey("dbo.Workshop", "AssistantId", "dbo.Instructor");
            DropForeignKey("dbo.Workshop", "InstructorId", "dbo.Instructor");
            DropForeignKey("dbo.Class", "AssistantId", "dbo.Instructor");
            DropForeignKey("dbo.Class", "InstructorId", "dbo.Instructor");
            DropForeignKey("dbo.Camp", "AssistantId", "dbo.Instructor");
            DropForeignKey("dbo.Camp", "InstructorId", "dbo.Instructor");
            DropForeignKey("dbo.Assign", "AssistantId", "dbo.Instructor");
            DropForeignKey("dbo.Assign", "InstructorId", "dbo.Instructor");
            DropForeignKey("dbo.Booking", "WorkshopId", "dbo.Workshop");
            DropForeignKey("dbo.Assign", "Workshop_Id1", "dbo.Workshop");
            DropForeignKey("dbo.Assign", "Workshop_Id", "dbo.Workshop");
            DropForeignKey("dbo.Parent", "LocationId", "dbo.Location");
            DropForeignKey("dbo.Class", "LocationId", "dbo.Location");
            DropForeignKey("dbo.Camp", "LocationId", "dbo.Location");
            DropForeignKey("dbo.Booking", "StudentId", "dbo.Student");
            DropForeignKey("dbo.GradeGroupDetail", "GradeGroupId", "dbo.GradeGroup");
            DropForeignKey("dbo.GradeGroupDetail", "GradeId", "dbo.Grade");
            DropForeignKey("dbo.Class", "GradeGroupId", "dbo.GradeGroup");
            DropForeignKey("dbo.Camp", "GradeGroupId", "dbo.GradeGroup");
            DropForeignKey("dbo.Booking", "ClassId", "dbo.Class");
            DropForeignKey("dbo.Assign", "Class_Id1", "dbo.Class");
            DropForeignKey("dbo.Assign", "Class_Id", "dbo.Class");
            DropForeignKey("dbo.Booking", "CampId", "dbo.Camp");
            DropForeignKey("dbo.Booking", "BirthdayId", "dbo.Birthday");
            DropForeignKey("dbo.Assign", "Camp_Id1", "dbo.Camp");
            DropForeignKey("dbo.Assign", "Camp_Id", "dbo.Camp");
            DropForeignKey("dbo.Assign", "BirthdayId", "dbo.Birthday");
            DropForeignKey("dbo.Admin", "AreaId", "dbo.Area");
            DropForeignKey("dbo.AdminTask", "AdminId", "dbo.Admin");
            DropForeignKey("dbo.AdminPaymentInfo", "AdminId", "dbo.Admin");
            DropForeignKey("dbo.AdminFrontend", "AdminId", "dbo.Admin");
            DropForeignKey("dbo.AdminFrontend", "FrontendId", "dbo.Frontend");
            DropForeignKey("dbo.AdminEmailTemplate", "AdminId", "dbo.Admin");
            DropIndex("dbo.GradeGroupDetail", new[] { "GradeGroupId" });
            DropIndex("dbo.GradeGroupDetail", new[] { "GradeId" });
            DropIndex("dbo.PrivateMessage", new[] { "ToAdminId" });
            DropIndex("dbo.PrivateMessage", new[] { "FromAdminId" });
            DropIndex("dbo.Coupon", new[] { "AdminId" });
            DropIndex("dbo.Instructor", new[] { "AreaId" });
            DropIndex("dbo.Workshop", new[] { "AssistantId" });
            DropIndex("dbo.Workshop", new[] { "InstructorId" });
            DropIndex("dbo.Workshop", new[] { "GradeGroupId" });
            DropIndex("dbo.Workshop", new[] { "LocationId" });
            DropIndex("dbo.Location", new[] { "AreaId" });
            DropIndex("dbo.Parent", new[] { "LocationId" });
            DropIndex("dbo.Student", new[] { "GradeId" });
            DropIndex("dbo.Student", new[] { "ParentId" });
            DropIndex("dbo.Class", new[] { "AssistantId" });
            DropIndex("dbo.Class", new[] { "InstructorId" });
            DropIndex("dbo.Class", new[] { "GradeGroupId" });
            DropIndex("dbo.Class", new[] { "LocationId" });
            DropIndex("dbo.Booking", new[] { "CouponId" });
            DropIndex("dbo.Booking", new[] { "StudentId" });
            DropIndex("dbo.Booking", new[] { "BirthdayId" });
            DropIndex("dbo.Booking", new[] { "WorkshopId" });
            DropIndex("dbo.Booking", new[] { "ClassId" });
            DropIndex("dbo.Booking", new[] { "CampId" });
            DropIndex("dbo.Camp", new[] { "AssistantId" });
            DropIndex("dbo.Camp", new[] { "InstructorId" });
            DropIndex("dbo.Camp", new[] { "GradeGroupId" });
            DropIndex("dbo.Camp", new[] { "LocationId" });
            DropIndex("dbo.Assign", new[] { "Workshop_Id1" });
            DropIndex("dbo.Assign", new[] { "Workshop_Id" });
            DropIndex("dbo.Assign", new[] { "Class_Id1" });
            DropIndex("dbo.Assign", new[] { "Class_Id" });
            DropIndex("dbo.Assign", new[] { "Camp_Id1" });
            DropIndex("dbo.Assign", new[] { "Camp_Id" });
            DropIndex("dbo.Assign", new[] { "WorkshopId" });
            DropIndex("dbo.Assign", new[] { "BirthdayId" });
            DropIndex("dbo.Assign", new[] { "CampId" });
            DropIndex("dbo.Assign", new[] { "ClassId" });
            DropIndex("dbo.Assign", new[] { "AssistantId" });
            DropIndex("dbo.Assign", new[] { "InstructorId" });
            DropIndex("dbo.Birthday", new[] { "AreaId" });
            DropIndex("dbo.AdminTask", new[] { "AdminId" });
            DropIndex("dbo.AdminPaymentInfo", new[] { "AdminId" });
            DropIndex("dbo.AdminFrontend", new[] { "FrontendId" });
            DropIndex("dbo.AdminFrontend", new[] { "AdminId" });
            DropIndex("dbo.Admin", new[] { "AreaId" });
            DropIndex("dbo.AdminEmailTemplate", new[] { "AdminId" });
            DropTable("dbo.GradeGroupDetail");
            DropTable("dbo.Payment");
            DropTable("dbo.EmailTemplate");
            DropTable("dbo.BookingShortInfoTemp");
            DropTable("dbo.APVariable");
            DropTable("dbo.PrivateMessage");
            DropTable("dbo.Coupon");
            DropTable("dbo.Instructor");
            DropTable("dbo.Workshop");
            DropTable("dbo.Location");
            DropTable("dbo.Parent");
            DropTable("dbo.Student");
            DropTable("dbo.Grade");
            DropTable("dbo.GradeGroup");
            DropTable("dbo.Class");
            DropTable("dbo.Booking");
            DropTable("dbo.Camp");
            DropTable("dbo.Assign");
            DropTable("dbo.Birthday");
            DropTable("dbo.Area");
            DropTable("dbo.AdminTask");
            DropTable("dbo.AdminPaymentInfo");
            DropTable("dbo.Frontend");
            DropTable("dbo.AdminFrontend");
            DropTable("dbo.Admin");
            DropTable("dbo.AdminEmailTemplate");
        }
    }
}
