-- Insert sample reviews
INSERT INTO Reviews (UserId, Rating, Content, CreatedDate, NumberOfFlags, IsHidden)
VALUES 
('6EF95E0F-D641-4922-A509-91B0A6758802', 4, 'Great service and atmosphere!', GETDATE(), 0, 0),
('6EF95E0F-D641-4922-A509-91B0A6758802', 5, 'Best place in town!', GETDATE(), 1, 0),
('346C225C-B625-4FD4-BF54-C8D98877F886', 3, 'Decent place, could be better.', GETDATE(), 2, 0),
('346C225C-B625-4FD4-BF54-C8D98877F886', 2, 'Not worth the price.', GETDATE(), 3, 1),
('66C33BA1-997F-4C1E-8412-FD6C432CFFD8', 5, 'Amazing experience!', GETDATE(), 0, 0),
('66C33BA1-997F-4C1E-8412-FD6C432CFFD8', 4, 'Good food and drinks.', GETDATE(), 1, 0);

-- Insert sample upgrade requests
INSERT INTO UpgradeRequests (RequestingUserIdentifier, RequestingUserDisplayName)
VALUES 
('6EF95E0F-D641-4922-A509-91B0A6758802', 'John Doe'),
('346C225C-B625-4FD4-BF54-C8D98877F886', 'Jane Smith'),
('66C33BA1-997F-4C1E-8412-FD6C432CFFD8', 'Bob Johnson'); 