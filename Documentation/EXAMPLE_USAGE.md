# SQL Database Schema Analysis and Visualization

Based on the database schema retrieved from `#GetDatabaseObjectsMetadata connectionName=P330_AdventureWorks2022`, this document provides a comprehensive analysis and visualization of the AdventureWorks2022 database structure.

## Database Overview

The AdventureWorks2022 database models a fictional bicycle manufacturer with comprehensive business data. It contains:

- **Tables**: Organized across multiple schemas representing different business areas
- **Views**: Providing simplified access to complex data relationships
- **Stored Procedures**: Implementing business logic and data manipulation
- **Functions**: Supporting calculations and data transformations
- **Schemas**: Organizing objects by business function

## Schema Organization

The database uses multiple schemas to organize objects by business function:

- **Person**: Customer and contact information
- **HumanResources**: Employee data and company structure
- **Production**: Products, inventory, and manufacturing
- **Purchasing**: Vendor relationships and purchasing transactions
- **Sales**: Customer orders, sales territories, and marketing
- **dbo**: System objects and miscellaneous functions

## Schema Diagrams

### Human Resources Schema

```mermaid
erDiagram
    HumanResources_Department {
        int DepartmentID PK
        nvarchar Name
        nvarchar GroupName
        datetime ModifiedDate
    }
    HumanResources_Employee {
        int BusinessEntityID PK
        nvarchar NationalIDNumber
        nvarchar LoginID
        nvarchar JobTitle
        date BirthDate
        nvarchar MaritalStatus
        nvarchar Gender
        date HireDate
        bit SalariedFlag
        smallint VacationHours
        smallint SickLeaveHours
        bit CurrentFlag
        uniqueidentifier rowguid
        datetime ModifiedDate
    }
    HumanResources_EmployeeDepartmentHistory {
        int BusinessEntityID PK
        int DepartmentID PK
        smallint ShiftID PK
        date StartDate PK
        date EndDate
        bit CurrentFlag
        datetime ModifiedDate
    }
    HumanResources_EmployeePayHistory {
        int BusinessEntityID PK
        date RateChangeDate PK
        money Rate
        tinyint PayFrequency
        datetime ModifiedDate
    }
    HumanResources_JobCandidate {
        int JobCandidateID PK
        int BusinessEntityID
        xml Resume
        datetime ModifiedDate
    }
    HumanResources_Shift {
        smallint ShiftID PK
        nvarchar Name
        time StartTime
        time EndTime
        datetime ModifiedDate
    }

    HumanResources_Department ||--o{ HumanResources_EmployeeDepartmentHistory : "has"
    HumanResources_Employee ||--o{ HumanResources_EmployeeDepartmentHistory : "has"
    HumanResources_Shift ||--o{ HumanResources_EmployeeDepartmentHistory : "has"
    HumanResources_Employee ||--o{ HumanResources_EmployeePayHistory : "has"
    HumanResources_Employee ||--o{ HumanResources_JobCandidate : "may be"
```

### Person Schema

```mermaid
erDiagram
    Person_Address {
        int AddressID PK
        nvarchar AddressLine1
        nvarchar AddressLine2
        nvarchar City
        int StateProvinceID FK
        nvarchar PostalCode
        geography SpatialLocation
        uniqueidentifier rowguid
        datetime ModifiedDate
    }
    Person_AddressType {
        int AddressTypeID PK
        nvarchar Name
        uniqueidentifier rowguid
        datetime ModifiedDate
    }
    Person_BusinessEntity {
        int BusinessEntityID PK
        uniqueidentifier rowguid
        datetime ModifiedDate
    }
    Person_BusinessEntityAddress {
        int BusinessEntityID PK
        int AddressID PK
        int AddressTypeID PK
        uniqueidentifier rowguid
        datetime ModifiedDate
    }
    Person_BusinessEntityContact {
        int BusinessEntityID PK
        int PersonID PK
        int ContactTypeID PK
        uniqueidentifier rowguid
        datetime ModifiedDate
    }
    Person_ContactType {
        int ContactTypeID PK
        nvarchar Name
        datetime ModifiedDate
    }
    Person_CountryRegion {
        nvarchar CountryRegionCode PK
        nvarchar Name
        datetime ModifiedDate
    }
    Person_EmailAddress {
        int BusinessEntityID PK
        int EmailAddressID PK
        nvarchar EmailAddress
        uniqueidentifier rowguid
        datetime ModifiedDate
    }
    Person_Password {
        int BusinessEntityID PK
        nvarchar PasswordHash
        nvarchar PasswordSalt
        uniqueidentifier rowguid
        datetime ModifiedDate
    }
    Person_Person {
        int BusinessEntityID PK
        nvarchar PersonType
        bit NameStyle
        nvarchar Title
        nvarchar FirstName
        nvarchar MiddleName
        nvarchar LastName
        nvarchar Suffix
        int EmailPromotion
        xml AdditionalContactInfo
        xml Demographics
        uniqueidentifier rowguid
        datetime ModifiedDate
    }
    Person_PersonPhone {
        int BusinessEntityID PK
        nvarchar PhoneNumber PK
        int PhoneNumberTypeID PK
        datetime ModifiedDate
    }
    Person_PhoneNumberType {
        int PhoneNumberTypeID PK
        nvarchar Name
        datetime ModifiedDate
    }
    Person_StateProvince {
        int StateProvinceID PK
        nvarchar StateProvinceCode
        nvarchar CountryRegionCode FK
        bit IsOnlyStateProvinceFlag
        nvarchar Name
        int TerritoryID FK
        uniqueidentifier rowguid
        datetime ModifiedDate
    }

    Person_BusinessEntity ||--o{ Person_BusinessEntityAddress : "has"
    Person_Address ||--o{ Person_BusinessEntityAddress : "used in"
    Person_AddressType ||--o{ Person_BusinessEntityAddress : "defines"
    Person_BusinessEntity ||--o{ Person_BusinessEntityContact : "has"
    Person_Person ||--o{ Person_BusinessEntityContact : "is"
    Person_ContactType ||--o{ Person_BusinessEntityContact : "defines"
    Person_BusinessEntity ||--o{ Person_Person : "can be"
    Person_Person ||--o{ Person_EmailAddress : "has"
    Person_Person ||--o| Person_Password : "may have"
    Person_Person ||--o{ Person_PersonPhone : "has"
    Person_PhoneNumberType ||--o{ Person_PersonPhone : "defines"
    Person_CountryRegion ||--o{ Person_StateProvince : "contains"
    Person_StateProvince ||--o{ Person_Address : "contains"
```

### Production Schema

```mermaid
erDiagram
    Production_BillOfMaterials {
        int BillOfMaterialsID PK
        int ProductAssemblyID
        int ComponentID FK
        datetime StartDate
        datetime EndDate
        nvarchar UnitMeasureCode FK
        smallint BOMLevel
        decimal PerAssemblyQty
        datetime ModifiedDate
    }
    Production_Culture {
        nvarchar CultureID PK
        nvarchar Name
        datetime ModifiedDate
    }
    Production_Document {
        hierarchyid DocumentNode PK
        int DocumentLevel
        nvarchar Title
        int Owner
        bit FolderFlag
        nvarchar FileName
        nvarchar FileExtension
        nvarchar Revision
        int ChangeNumber
        tinyint Status
        nvarchar DocumentSummary
        varbinary Document
        uniqueidentifier rowguid
        datetime ModifiedDate
    }
    Production_Illustration {
        int IllustrationID PK
        xml Diagram
        datetime ModifiedDate
    }
    Production_Location {
        smallint LocationID PK
        nvarchar Name
        decimal CostRate
        decimal Availability
        datetime ModifiedDate
    }
    Production_Product {
        int ProductID PK
        nvarchar Name
        nvarchar ProductNumber
        bit MakeFlag
        bit FinishedGoodsFlag
        nvarchar Color
        smallint SafetyStockLevel
        smallint ReorderPoint
        money StandardCost
        money ListPrice
        nvarchar Size
        nvarchar SizeUnitMeasureCode FK
        nvarchar WeightUnitMeasureCode FK
        decimal Weight
        int DaysToManufacture
        nvarchar ProductLine
        nvarchar Class
        nvarchar Style
        int ProductSubcategoryID FK
        int ProductModelID FK
        datetime SellStartDate
        datetime SellEndDate
        datetime DiscontinuedDate
        uniqueidentifier rowguid
        datetime ModifiedDate
    }
    Production_ProductCategory {
        int ProductCategoryID PK
        nvarchar Name
        uniqueidentifier rowguid
        datetime ModifiedDate
    }
    Production_ProductCostHistory {
        int ProductID PK
        datetime StartDate PK
        datetime EndDate
        money StandardCost
        datetime ModifiedDate
    }
    Production_ProductDescription {
        int ProductDescriptionID PK
        nvarchar Description
        uniqueidentifier rowguid
        datetime ModifiedDate
    }
    Production_ProductDocument {
        int ProductID PK
        hierarchyid DocumentNode PK
        datetime ModifiedDate
    }
    Production_ProductInventory {
        int ProductID PK
        smallint LocationID PK
        nvarchar Shelf
        tinyint Bin
        smallint Quantity
        uniqueidentifier rowguid
        datetime ModifiedDate
    }
    Production_ProductListPriceHistory {
        int ProductID PK
        datetime StartDate PK
        datetime EndDate
        money ListPrice
        datetime ModifiedDate
    }
    Production_ProductModel {
        int ProductModelID PK
        nvarchar Name
        xml CatalogDescription
        xml Instructions
        uniqueidentifier rowguid
        datetime ModifiedDate
    }
    Production_ProductModelIllustration {
        int ProductModelID PK
        int IllustrationID PK
        datetime ModifiedDate
    }
    Production_ProductModelProductDescriptionCulture {
        int ProductModelID PK
        int ProductDescriptionID PK
        nvarchar CultureID PK
        datetime ModifiedDate
    }
    Production_ProductPhoto {
        int ProductPhotoID PK
        varbinary ThumbNailPhoto
        nvarchar ThumbnailPhotoFileName
        varbinary LargePhoto
        nvarchar LargePhotoFileName
        datetime ModifiedDate
    }
    Production_ProductProductPhoto {
        int ProductID PK
        int ProductPhotoID PK
        bit Primary
        datetime ModifiedDate
    }
    Production_ProductReview {
        int ProductReviewID PK
        int ProductID
        nvarchar ReviewerName
        datetime ReviewDate
        nvarchar EmailAddress
        int Rating
        nvarchar Comments
        datetime ModifiedDate
    }
    Production_ProductSubcategory {
        int ProductSubcategoryID PK
        int ProductCategoryID FK
        nvarchar Name
        uniqueidentifier rowguid
        datetime ModifiedDate
    }
    Production_ScrapReason {
        smallint ScrapReasonID PK
        nvarchar Name
        datetime ModifiedDate
    }
    Production_TransactionHistory {
        int TransactionID PK
        int ProductID
        int ReferenceOrderID
        int ReferenceOrderLineID
        datetime TransactionDate
        nchar TransactionType
        int Quantity
        money ActualCost
        datetime ModifiedDate
    }
    Production_TransactionHistoryArchive {
        int TransactionID PK
        int ProductID
        int ReferenceOrderID
        int ReferenceOrderLineID
        datetime TransactionDate
        nchar TransactionType
        int Quantity
        money ActualCost
        datetime ModifiedDate
    }
    Production_UnitMeasure {
        nvarchar UnitMeasureCode PK
        nvarchar Name
        datetime ModifiedDate
    }
    Production_WorkOrder {
        int WorkOrderID PK
        int ProductID
        int OrderQty
        int StockedQty
        smallint ScrappedQty
        datetime StartDate
        datetime EndDate
        datetime DueDate
        smallint ScrapReasonID
        datetime ModifiedDate
    }
    Production_WorkOrderRouting {
        int WorkOrderID PK
        int ProductID PK
        smallint OperationSequence PK
        smallint LocationID
        datetime ScheduledStartDate
        datetime ScheduledEndDate
        datetime ActualStartDate
        datetime ActualEndDate
        decimal ActualResourceHrs
        money PlannedCost
        money ActualCost
        datetime ModifiedDate
    }

    Production_Product ||--o{ Production_BillOfMaterials : "is component in"
    Production_Product ||--o{ Production_BillOfMaterials : "is assembly of"
    Production_UnitMeasure ||--o{ Production_BillOfMaterials : "defines"
    Production_UnitMeasure ||--o{ Production_Product : "defines weight"
    Production_UnitMeasure ||--o{ Production_Product : "defines size"
    Production_ProductSubcategory ||--o{ Production_Product : "classifies"
    Production_ProductModel ||--o{ Production_Product : "describes"
    Production_ProductCategory ||--o{ Production_ProductSubcategory : "contains"
    Production_Product ||--o{ Production_ProductCostHistory : "has"
    Production_Product ||--o{ Production_ProductDocument : "has"
    Production_Document ||--o{ Production_ProductDocument : "describes"
    Production_Product ||--o{ Production_ProductInventory : "has"
    Production_Location ||--o{ Production_ProductInventory : "stores"
    Production_Product ||--o{ Production_ProductListPriceHistory : "has"
    Production_ProductModel ||--o{ Production_ProductModelIllustration : "has"
    Production_Illustration ||--o{ Production_ProductModelIllustration : "used in"
    Production_ProductModel ||--o{ Production_ProductModelProductDescriptionCulture : "has"
    Production_ProductDescription ||--o{ Production_ProductModelProductDescriptionCulture : "used in"
    Production_Culture ||--o{ Production_ProductModelProductDescriptionCulture : "used in"
    Production_Product ||--o{ Production_ProductProductPhoto : "has"
    Production_ProductPhoto ||--o{ Production_ProductProductPhoto : "used by"
    Production_Product ||--o{ Production_ProductReview : "has"
    Production_Product ||--o{ Production_TransactionHistory : "has"
    Production_Product ||--o{ Production_TransactionHistoryArchive : "has"
    Production_Product ||--o{ Production_WorkOrder : "produced by"
    Production_WorkOrder ||--o{ Production_WorkOrderRouting : "has"
    Production_Location ||--o{ Production_WorkOrderRouting : "used in"
    Production_ScrapReason ||--o{ Production_WorkOrder : "explains scrap"
```

### Purchasing Schema

```mermaid
erDiagram
    Purchasing_ProductVendor {
        int ProductID PK
        int BusinessEntityID PK
        int AverageLeadTime
        money StandardPrice
        money LastReceiptCost
        datetime LastReceiptDate
        int MinOrderQty
        int MaxOrderQty
        int OnOrderQty
        nvarchar UnitMeasureCode FK
        datetime ModifiedDate
    }
    Purchasing_PurchaseOrderDetail {
        int PurchaseOrderID PK
        int PurchaseOrderDetailID PK
        datetime DueDate
        smallint OrderQty
        int ProductID FK
        money UnitPrice
        money LineTotal
        decimal ReceivedQty
        decimal RejectedQty
        decimal StockedQty
        datetime ModifiedDate
    }
    Purchasing_PurchaseOrderHeader {
        int PurchaseOrderID PK
        tinyint RevisionNumber
        tinyint Status
        int EmployeeID FK
        int VendorID FK
        int ShipMethodID FK
        datetime OrderDate
        datetime ShipDate
        money SubTotal
        money TaxAmt
        money Freight
        money TotalDue
        datetime ModifiedDate
    }
    Purchasing_ShipMethod {
        int ShipMethodID PK
        nvarchar Name
        money ShipBase
        money ShipRate
        uniqueidentifier rowguid
        datetime ModifiedDate
    }
    Purchasing_Vendor {
        int BusinessEntityID PK
        nvarchar AccountNumber
        nvarchar Name
        tinyint CreditRating
        bit PreferredVendorStatus
        bit ActiveFlag
        nvarchar PurchasingWebServiceURL
        datetime ModifiedDate
    }

    Purchasing_Vendor ||--o{ Purchasing_ProductVendor : "supplies"
    Production_Product ||--o{ Purchasing_ProductVendor : "supplied by"
    Production_UnitMeasure ||--o{ Purchasing_ProductVendor : "measured in"
    Purchasing_PurchaseOrderHeader ||--o{ Purchasing_PurchaseOrderDetail : "contains"
    Production_Product ||--o{ Purchasing_PurchaseOrderDetail : "ordered in"
    Purchasing_Vendor ||--o{ Purchasing_PurchaseOrderHeader : "receives"
    Purchasing_ShipMethod ||--o{ Purchasing_PurchaseOrderHeader : "used by"
    HumanResources_Employee ||--o{ Purchasing_PurchaseOrderHeader : "creates"
```

### Sales Schema

```mermaid
erDiagram
    Sales_CountryRegionCurrency {
        nvarchar CountryRegionCode PK
        nvarchar CurrencyCode PK
        datetime ModifiedDate
    }
    Sales_CreditCard {
        int CreditCardID PK
        nvarchar CardType
        nvarchar CardNumber
        tinyint ExpMonth
        smallint ExpYear
        datetime ModifiedDate
    }
    Sales_Currency {
        nvarchar CurrencyCode PK
        nvarchar Name
        datetime ModifiedDate
    }
    Sales_CurrencyRate {
        int CurrencyRateID PK
        datetime CurrencyRateDate
        nvarchar FromCurrencyCode FK
        nvarchar ToCurrencyCode FK
        money AverageRate
        money EndOfDayRate
        datetime ModifiedDate
    }
    Sales_Customer {
        int CustomerID PK
        int PersonID FK
        int StoreID FK
        int TerritoryID FK
        nvarchar AccountNumber
        uniqueidentifier rowguid
        datetime ModifiedDate
    }
    Sales_PersonCreditCard {
        int BusinessEntityID PK
        int CreditCardID PK
        datetime ModifiedDate
    }
    Sales_SalesOrderDetail {
        int SalesOrderID PK
        int SalesOrderDetailID PK
        nvarchar CarrierTrackingNumber
        smallint OrderQty
        int ProductID FK
        int SpecialOfferID FK
        money UnitPrice
        money UnitPriceDiscount
        money LineTotal
        uniqueidentifier rowguid
        datetime ModifiedDate
    }
    Sales_SalesOrderHeader {
        int SalesOrderID PK
        tinyint RevisionNumber
        datetime OrderDate
        datetime DueDate
        datetime ShipDate
        tinyint Status
        bit OnlineOrderFlag
        nvarchar SalesOrderNumber
        nvarchar PurchaseOrderNumber
        nvarchar AccountNumber
        int CustomerID FK
        int SalesPersonID FK
        int TerritoryID FK
        int BillToAddressID FK
        int ShipToAddressID FK
        int ShipMethodID FK
        int CreditCardID FK
        nvarchar CreditCardApprovalCode
        int CurrencyRateID FK
        money SubTotal
        money TaxAmt
        money Freight
        money TotalDue
        nvarchar Comment
        uniqueidentifier rowguid
        datetime ModifiedDate
    }
    Sales_SalesOrderHeaderSalesReason {
        int SalesOrderID PK
        int SalesReasonID PK
        datetime ModifiedDate
    }
    Sales_SalesPerson {
        int BusinessEntityID PK
        int TerritoryID FK
        money SalesQuota
        money Bonus
        money CommissionPct
        money SalesYTD
        money SalesLastYear
        uniqueidentifier rowguid
        datetime ModifiedDate
    }
    Sales_SalesPersonQuotaHistory {
        int BusinessEntityID PK
        datetime QuotaDate PK
        money SalesQuota
        uniqueidentifier rowguid
        datetime ModifiedDate
    }
    Sales_SalesReason {
        int SalesReasonID PK
        nvarchar Name
        nvarchar ReasonType
        datetime ModifiedDate
    }
    Sales_SalesTaxRate {
        int SalesTaxRateID PK
        int StateProvinceID
        tinyint TaxType
        decimal TaxRate
        nvarchar Name
        uniqueidentifier rowguid
        datetime ModifiedDate
    }
    Sales_SalesTerritory {
        int TerritoryID PK
        nvarchar Name
        nvarchar CountryRegionCode FK
        nvarchar Group
        money SalesYTD
        money SalesLastYear
        money CostYTD
        money CostLastYear
        uniqueidentifier rowguid
        datetime ModifiedDate
    }
    Sales_SalesTerritoryHistory {
        int BusinessEntityID PK
        int TerritoryID PK
        datetime StartDate PK
        datetime EndDate
        uniqueidentifier rowguid
        datetime ModifiedDate
    }
    Sales_ShoppingCartItem {
        int ShoppingCartItemID PK
        nvarchar ShoppingCartID
        int Quantity
        int ProductID FK
        datetime DateCreated
        datetime ModifiedDate
    }
    Sales_SpecialOffer {
        int SpecialOfferID PK
        nvarchar Description
        decimal DiscountPct
        nvarchar Type
        nvarchar Category
        datetime StartDate
        datetime EndDate
        int MinQty
        int MaxQty
        uniqueidentifier rowguid
        datetime ModifiedDate
    }
    Sales_SpecialOfferProduct {
        int SpecialOfferID PK
        int ProductID PK
        uniqueidentifier rowguid
        datetime ModifiedDate
    }
    Sales_Store {
        int BusinessEntityID PK
        nvarchar Name
        int SalesPersonID FK
        xml Demographics
        uniqueidentifier rowguid
        datetime ModifiedDate
    }

    Person_CountryRegion ||--o{ Sales_CountryRegionCurrency : "has"
    Sales_Currency ||--o{ Sales_CountryRegionCurrency : "used in"
    Sales_Currency ||--o{ Sales_CurrencyRate : "from"
    Sales_Currency ||--o{ Sales_CurrencyRate : "to"
    Person_Person ||--o{ Sales_Customer : "can be"
    Sales_Store ||--o{ Sales_Customer : "can be"
    Sales_SalesTerritory ||--o{ Sales_Customer : "belongs to"
    Person_Person ||--o{ Sales_PersonCreditCard : "has"
    Sales_CreditCard ||--o{ Sales_PersonCreditCard : "belongs to"
    Sales_SalesOrderHeader ||--o{ Sales_SalesOrderDetail : "contains"
    Production_Product ||--o{ Sales_SalesOrderDetail : "ordered in"
    Sales_SpecialOfferProduct ||--o{ Sales_SalesOrderDetail : "applied to"
    Sales_Customer ||--o{ Sales_SalesOrderHeader : "places"
    Sales_SalesPerson ||--o{ Sales_SalesOrderHeader : "manages"
    Sales_SalesTerritory ||--o{ Sales_SalesOrderHeader : "covers"
    Person_Address ||--o{ Sales_SalesOrderHeader : "ships to"
    Person_Address ||--o{ Sales_SalesOrderHeader : "bills to"
    Purchasing_ShipMethod ||--o{ Sales_SalesOrderHeader : "ships via"
    Sales_CreditCard ||--o{ Sales_SalesOrderHeader : "paid with"
    Sales_CurrencyRate ||--o{ Sales_SalesOrderHeader : "converted using"
    Sales_SalesOrderHeader ||--o{ Sales_SalesOrderHeaderSalesReason : "has"
    Sales_SalesReason ||--o{ Sales_SalesOrderHeaderSalesReason : "applies to"
    Sales_SalesPerson ||--o{ Sales_SalesPersonQuotaHistory : "has"
    Person_StateProvince ||--o{ Sales_SalesTaxRate : "has"
    Person_CountryRegion ||--o{ Sales_SalesTerritory : "contains"
    Sales_SalesPerson ||--o{ Sales_SalesTerritoryHistory : "assigned to"
    Sales_SalesTerritory ||--o{ Sales_SalesTerritoryHistory : "covered by"
    Production_Product ||--o{ Sales_ShoppingCartItem : "in cart"
    Sales_SpecialOffer ||--o{ Sales_SpecialOfferProduct : "applies to"
    Production_Product ||--o{ Sales_SpecialOfferProduct : "has"
    Person_BusinessEntity ||--o{ Sales_Store : "can be"
    Sales_SalesPerson ||--o{ Sales_Store : "assigned to"
```

## Views

The AdventureWorks database contains numerous views that simplify complex data access:

### Human Resources Views

- **vEmployee**: Complete employee information
- **vEmployeeDepartment**: Employees with department details
- **vEmployeeDepartmentHistory**: Historical department assignments
- **vJobCandidate**: Job candidate information with resume details
- **vJobCandidateEducation**: Education history from candidate resumes
- **vJobCandidateEmployment**: Employment history from candidate resumes

### Person Views

- **vAdditionalContactInfo**: Extended contact information
- **vStateProvinceCountryRegion**: State and country information

### Production Views

- **vProductAndDescription**: Products with localized descriptions
- **vProductModelCatalogDescription**: Product model catalog information
- **vProductModelInstructions**: Manufacturing instructions for products

### Sales Views

- **vIndividualCustomer**: Customer information for individuals
- **vPersonDemographics**: Customer demographic information
- **vSalesPerson**: Sales staff information with territories
- **vSalesPersonSalesByFiscalYears**: Sales performance by fiscal year
- **vStoreWithDemographics**: Store information with demographics
- **vStoreWithContacts**: Store information with contact details
- **vStoreWithAddresses**: Store information with address details

## Stored Procedures

The database contains stored procedures for various business operations:

### Human Resources Procedures

- **uspUpdateEmployeeHireInfo**: Updates employee hire information
- **uspUpdateEmployeeLogin**: Updates employee login information
- **uspUpdateEmployeePersonalInfo**: Updates employee personal details

### Database Management Procedures

- **uspPrintError**: Prints error information
- **uspLogError**: Logs error details
- **uspGetWhereUsedProductID**: Finds where a product is used
- **uspGetManagerEmployees**: Gets employees reporting to a manager
- **uspGetEmployeeManagers**: Gets managers for an employee
- **uspGetBillOfMaterials**: Retrieves bill of materials for a product

### Sales Procedures

- **uspGetOrderTrackingBySalesOrderID**: Tracks order status

## Functions

The database includes various functions for calculations and data transformation:

### Scalar Functions

- **ufnGetAccountingEndDate**: Calculates accounting period end date
- **ufnGetAccountingStartDate**: Calculates accounting period start date
- **ufnGetProductDealerPrice**: Determines dealer price for a product
- **ufnGetProductListPrice**: Gets list price for a product on a date
- **ufnGetProductStandardCost**: Gets standard cost for a product on a date
- **ufnGetStock**: Gets current inventory level for a product
- **ufnGetDocumentStatusText**: Converts document status code to text
- **ufnGetPurchaseOrderStatusText**: Converts purchase order status to text
- **ufnGetSalesOrderStatusText**: Converts sales order status to text

### Table-Valued Functions

- **ufnGetContactInformation**: Returns contact information for a person
- **ufnGetCustomerInformation**: Returns customer information

## Data Flow Diagram

```mermaid
flowchart TD
    Person[Person Schema] <--> HR[Human Resources Schema]
    Person <--> Sales[Sales Schema]
    Person <--> Purchasing[Purchasing Schema]

    Production[Production Schema] --> Sales
    Production --> Purchasing

    Sales --> Purchasing

    HR --> Sales

    subgraph CoreEntities
        Person_Person[Person]
        Person_BusinessEntity[Business Entity]
        Person_Address[Address]
        Production_Product[Product]
        HumanResources_Employee[Employee]
    end

    subgraph Transactions
        Sales_SalesOrderHeader[Sales Orders]
        Purchasing_PurchaseOrderHeader[Purchase Orders]
        Production_WorkOrder[Work Orders]
    end

    Person_Person --> Sales_Customer
    Person_BusinessEntity --> Sales_Store
    Person_BusinessEntity --> Purchasing_Vendor

    HumanResources_Employee --> Sales_SalesPerson
    HumanResources_Employee --> Purchasing_PurchaseOrderHeader

    Production_Product --> Sales_SalesOrderDetail
    Production_Product --> Purchasing_PurchaseOrderDetail
    Production_Product --> Production_WorkOrder

    Sales_Customer --> Sales_SalesOrderHeader
    Purchasing_Vendor --> Purchasing_PurchaseOrderHeader
```

## Key Insights

1. **Comprehensive Business Model**: The AdventureWorks database models a complete business operation from manufacturing to sales, with interconnected schemas representing different business areas.

2. **Rich Customer Data**: The Person schema maintains detailed customer information, supporting both individual and store customers.

3. **Production Tracking**: The Production schema contains comprehensive product data, including bill of materials, inventory tracking, and work orders.

4. **Sales Pipeline**: The Sales schema tracks everything from shopping carts to completed orders, with support for special offers and sales territories.

5. **Human Resources Management**: The HR schema manages employee data, department structure, and job candidates.

6. **Multi-currency Support**: Built-in support for different currencies and exchange rates for international business.

7. **Territorial Organization**: Sales are organized by territories with historical tracking of sales performance.

8. **Vendor Management**: The Purchasing schema handles vendor relationships and purchase orders.

The AdventureWorks database provides a realistic model of a mid-sized manufacturing company's data structure, with proper normalization and relationships across multiple business domains.
