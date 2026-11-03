-- Mendera Qoci Kebele DFMS Database Backup
-- Generated: 2026-10-08 16:16:30
-- Host: Local Server

-- Table: SubKebeles (4 records)
INSERT INTO SubKebeles (SubKebeleId, SubKebeleName, Description, IsActive) VALUES (1, 'Sub-Kebele 01 (Central)', 'Central administrative zone of Mendera Qoci', 1);
INSERT INTO SubKebeles (SubKebeleId, SubKebeleName, Description, IsActive) VALUES (2, 'Sub-Kebele 02 (North)', 'Northern residential community area', 1);
INSERT INTO SubKebeles (SubKebeleId, SubKebeleName, Description, IsActive) VALUES (3, 'Sub-Kebele 03 (East)', 'Eastern market & agricultural zone', 1);
INSERT INTO SubKebeles (SubKebeleId, SubKebeleName, Description, IsActive) VALUES (4, 'Sub-Kebele 04 (South)', 'Southern expanding suburb district', 1);

-- Table: Residents (4 records)
INSERT INTO Residents (ResidentId, FirstName, LastName, DateOfBirth, Gender, PhoneNumber, SubKebeleId, HouseholdHead, RegistrationDate, Status) VALUES (1, 'Abebe', 'Bikila', '1985-04-12', 'Male', '0911223344', 1, 'Abebe Bikila', '2024-10-07 23:05:50', 'Active');
INSERT INTO Residents (ResidentId, FirstName, LastName, DateOfBirth, Gender, PhoneNumber, SubKebeleId, HouseholdHead, RegistrationDate, Status) VALUES (2, 'Chaltu', 'Gemechu', '1992-08-25', 'Female', '0922334455', 2, 'Gemechu Daba', '2025-10-07 23:05:50', 'Active');
INSERT INTO Residents (ResidentId, FirstName, LastName, DateOfBirth, Gender, PhoneNumber, SubKebeleId, HouseholdHead, RegistrationDate, Status) VALUES (3, 'Kebede', 'Tadesse', '1978-11-30', 'Male', '0933445566', 3, 'Kebede Tadesse', '2026-02-07 23:05:50', 'Active');
INSERT INTO Residents (ResidentId, FirstName, LastName, DateOfBirth, Gender, PhoneNumber, SubKebeleId, HouseholdHead, RegistrationDate, Status) VALUES (4, 'Aster', 'Aweke', '1995-02-14', 'Female', '0944556677', 1, 'Aweke Bekele', '2026-07-07 23:05:50', 'Active');

-- Table: KebeleIdRecords (7 records)
INSERT INTO KebeleIdRecords (IdRecordId, ResidentId, IdNumber, IssueDate, ExpiryDate, IdStatus, IssuedByUserId, Remarks) VALUES (1, 1, 'KID-MQ-2024-001', '2025-10-07', '2026-10-22', 'Expired', '39feea6f-8175-4589-a05a-ba66c6eb6854', 'Initial issuance');
INSERT INTO KebeleIdRecords (IdRecordId, ResidentId, IdNumber, IssueDate, ExpiryDate, IdStatus, IssuedByUserId, Remarks) VALUES (2, 2, 'KID-MQ-2023-089', '2023-10-07', '2025-10-07', 'Expired', '548afea0-bfe7-496b-8677-8e48dbfdf180', 'Old ID record');
INSERT INTO KebeleIdRecords (IdRecordId, ResidentId, IdNumber, IssueDate, ExpiryDate, IdStatus, IssuedByUserId, Remarks) VALUES (3, 2, 'KID-MQ-2025-042', '2026-08-07', '2028-10-07', 'Active', '548afea0-bfe7-496b-8677-8e48dbfdf180', 'Renewed ID record');
INSERT INTO KebeleIdRecords (IdRecordId, ResidentId, IdNumber, IssueDate, ExpiryDate, IdStatus, IssuedByUserId, Remarks) VALUES (4, 3, 'KID-MQ-2024-112', '2026-04-07', '2028-04-07', 'Active', '39feea6f-8175-4589-a05a-ba66c6eb6854', 'Issued upon verification');
INSERT INTO KebeleIdRecords (IdRecordId, ResidentId, IdNumber, IssueDate, ExpiryDate, IdStatus, IssuedByUserId, Remarks) VALUES (5, 1, 'KID-MQ-2026-213', '2026-10-07', '2028-10-07', 'Active', '548afea0-bfe7-496b-8677-8e48dbfdf180', 'Renewed from previous ID #KID-MQ-2024-001');
INSERT INTO KebeleIdRecords (IdRecordId, ResidentId, IdNumber, IssueDate, ExpiryDate, IdStatus, IssuedByUserId, Remarks) VALUES (6, 2, 'KID-MQ-2026-308', '2026-10-07', '2028-10-07', 'Expired', '548afea0-bfe7-496b-8677-8e48dbfdf180', '');
INSERT INTO KebeleIdRecords (IdRecordId, ResidentId, IdNumber, IssueDate, ExpiryDate, IdStatus, IssuedByUserId, Remarks) VALUES (7, 2, 'KID-MQ-2026-279', '2026-10-08', '2028-10-08', 'Active', '39feea6f-8175-4589-a05a-ba66c6eb6854', 'Renewed from previous ID #KID-MQ-2026-308');

-- Table: Correspondences (3 records)
INSERT INTO Correspondences (CorrespondenceId, ResidentId, DocumentType, IssueDate, Purpose, IssuedByUserId, ReferenceNumber) VALUES (1, 1, 'Residence Certificate', '2026-05-07', 'Bank Account Opening Requirement', '39feea6f-8175-4589-a05a-ba66c6eb6854', 'COR-MQ-2025-101');
INSERT INTO Correspondences (CorrespondenceId, ResidentId, DocumentType, IssueDate, Purpose, IssuedByUserId, ReferenceNumber) VALUES (2, 2, 'Support Letter', '2026-09-07', 'Higher Education Scholarship Application', '548afea0-bfe7-496b-8677-8e48dbfdf180', 'COR-MQ-2025-204');
INSERT INTO Correspondences (CorrespondenceId, ResidentId, DocumentType, IssueDate, Purpose, IssuedByUserId, ReferenceNumber) VALUES (3, 4, 'Other Official Letter', '2026-10-08', 'his residence certificate is issued to confirm that Abebe Bikila is a legal resident of Mendera Qoci Kebele for all official and legal purposes.

You can adjust the details to fit your specific needs. If you provide more specific information, I can customize the filled form further for you.', '39feea6f-8175-4589-a05a-ba66c6eb6854', 'COR-MQ-2026-656');

