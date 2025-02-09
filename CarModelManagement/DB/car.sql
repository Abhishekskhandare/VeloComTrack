CREATE TABLE Cars (
    Id INT PRIMARY KEY IDENTITY,
    Brand NVARCHAR(50) NOT NULL CHECK (Brand IN ('Audi', 'Jaguar', 'Land Rover', 'Renault')),
    Class NVARCHAR(50) NOT NULL CHECK (Class IN ('A-Class', 'B-Class', 'C-Class')),
    ModelName NVARCHAR(100) NOT NULL,
    ModelCode NVARCHAR(10) NOT NULL CHECK (ModelCode LIKE '[A-Z0-9][A-Z0-9][A-Z0-9][A-Z0-9][A-Z0-9][A-Z0-9][A-Z0-9][A-Z0-9][A-Z0-9][A-Z0-9]'),
    Description NVARCHAR(MAX) NOT NULL,
    Features NVARCHAR(MAX) NOT NULL,
    Price DECIMAL(18,2) NOT NULL CHECK (Price > 0),
    DateOfManufacturing DATETIME NOT NULL,
    Active BIT NOT NULL DEFAULT 1,
    SortOrder INT NOT NULL CHECK (SortOrder >= 0)
);

CREATE TABLE CarImages (
    Id INT PRIMARY KEY IDENTITY,
    CarId INT FOREIGN KEY REFERENCES Cars(Id) ON DELETE CASCADE,
    ImagePath NVARCHAR(255) NOT NULL
);
