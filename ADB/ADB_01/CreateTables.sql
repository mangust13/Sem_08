USE AW_Marketing;
GO

-- Remove tables if they already exist
IF EXISTS (SELECT * FROM sys.tables t
           JOIN sys.schemas s ON t.schema_id = s.schema_id
           WHERE t.[Name] = 'SpecialOffers'
           AND s.[Name] = 'Promotions')
DROP TABLE Promotions.SpecialOffers

IF EXISTS (SELECT * FROM sys.tables t
           JOIN sys.schemas s ON t.schema_id = s.schema_id
           WHERE t.[Name] = 'SpecialOffers'
           AND s.[Name] = 'PastPromotions')
    DROP TABLE PastPromotions.SpecialOffers

-- Create table for special offers. No filegroup specified
CREATE TABLE Promotions.SpecialOffers
(OfferID int IDENTITY PRIMARY KEY,
 Description nvarchar(200),
 StartDate datetime,
 EndDate datetime,
 DiscountPercent decimal)

-- Create table for archived offers on the ArchivedData filegroup
CREATE TABLE PastPromotions.SpecialOffers
(OfferID int IDENTITY PRIMARY KEY,
 Description nvarchar(200),
 StartDate datetime,
 EndDate datetime,
 DiscountPercent decimal)
ON ArchivedData
