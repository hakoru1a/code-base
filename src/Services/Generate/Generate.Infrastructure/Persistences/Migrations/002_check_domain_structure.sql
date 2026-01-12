-- ============================================================================
-- SCRIPT KIỂM TRA CẤU TRÚC DATABASE - DOMAIN STRUCTURE VALIDATION
-- ============================================================================
-- Mục đích: Kiểm tra cấu trúc database có đúng với domain model không
-- Generated for: Generate.Domain entities
-- Date: Generated script
-- ============================================================================

-- Thiết lập biến để lưu kết quả
SET @errors = 0;
SET @warnings = 0;

-- ============================================================================
-- 1. KIỂM TRA SỰ TỒN TẠI CỦA CÁC BẢNG
-- ============================================================================

SELECT '=== KIỂM TRA BẢNG (TABLES) ===' AS 'CHECK';

-- Kiểm tra bảng CATEGORY
SELECT 
    CASE 
        WHEN COUNT(*) > 0 THEN '✓ Bảng CATEGORY tồn tại'
        ELSE CONCAT('✗ LỖI: Bảng CATEGORY không tồn tại')
    END AS 'Status'
FROM information_schema.tables 
WHERE table_schema = DATABASE() AND table_name = 'CATEGORY';

-- Kiểm tra bảng PRODUCT
SELECT 
    CASE 
        WHEN COUNT(*) > 0 THEN '✓ Bảng PRODUCT tồn tại'
        ELSE CONCAT('✗ LỖI: Bảng PRODUCT không tồn tại')
    END AS 'Status'
FROM information_schema.tables 
WHERE table_schema = DATABASE() AND table_name = 'PRODUCT';

-- Kiểm tra bảng PRODUCT_DETAIL
SELECT 
    CASE 
        WHEN COUNT(*) > 0 THEN '✓ Bảng PRODUCT_DETAIL tồn tại'
        ELSE CONCAT('✗ LỖI: Bảng PRODUCT_DETAIL không tồn tại')
    END AS 'Status'
FROM information_schema.tables 
WHERE table_schema = DATABASE() AND table_name = 'PRODUCT_DETAIL';

-- Kiểm tra bảng ORDER
SELECT 
    CASE 
        WHEN COUNT(*) > 0 THEN '✓ Bảng ORDER tồn tại'
        ELSE CONCAT('✗ LỖI: Bảng ORDER không tồn tại')
    END AS 'Status'
FROM information_schema.tables 
WHERE table_schema = DATABASE() AND table_name = 'ORDER';

-- Kiểm tra bảng ORDER_ITEM
SELECT 
    CASE 
        WHEN COUNT(*) > 0 THEN '✓ Bảng ORDER_ITEM tồn tại'
        ELSE CONCAT('✗ LỖI: Bảng ORDER_ITEM không tồn tại')
    END AS 'Status'
FROM information_schema.tables 
WHERE table_schema = DATABASE() AND table_name = 'ORDER_ITEM';

-- ============================================================================
-- 2. KIỂM TRA CẤU TRÚC CỘT CỦA CÁC BẢNG
-- ============================================================================

SELECT '=== KIỂM TRA CỘT BẢNG CATEGORY ===' AS 'CHECK';

SELECT 
    COLUMN_NAME AS 'Column',
    DATA_TYPE AS 'Type',
    IS_NULLABLE AS 'Nullable',
    COLUMN_DEFAULT AS 'Default',
    CASE 
        WHEN COLUMN_NAME = 'ID' AND DATA_TYPE = 'bigint' AND COLUMN_KEY = 'PRI' THEN '✓'
        WHEN COLUMN_NAME = 'NAME' AND DATA_TYPE = 'varchar' AND IS_NULLABLE = 'NO' THEN '✓'
        WHEN COLUMN_NAME = 'CREATED_DATE' AND DATA_TYPE = 'timestamp' AND IS_NULLABLE = 'NO' THEN '✓'
        WHEN COLUMN_NAME = 'LAST_MODIFIED_DATE' AND DATA_TYPE = 'timestamp' THEN '✓'
        WHEN COLUMN_NAME = 'CREATED_BY' AND DATA_TYPE = 'bigint' AND IS_NULLABLE = 'NO' THEN '✓'
        WHEN COLUMN_NAME = 'LAST_MODIFIED_BY' AND DATA_TYPE = 'bigint' THEN '✓'
        ELSE '?'
    END AS 'Status'
FROM information_schema.columns
WHERE table_schema = DATABASE() AND table_name = 'CATEGORY'
ORDER BY ORDINAL_POSITION;

SELECT '=== KIỂM TRA CỘT BẢNG PRODUCT ===' AS 'CHECK';

SELECT 
    COLUMN_NAME AS 'Column',
    DATA_TYPE AS 'Type',
    IS_NULLABLE AS 'Nullable',
    COLUMN_DEFAULT AS 'Default',
    CASE 
        WHEN COLUMN_NAME = 'ID' AND DATA_TYPE = 'bigint' AND COLUMN_KEY = 'PRI' THEN '✓'
        WHEN COLUMN_NAME = 'NAME' AND DATA_TYPE = 'varchar' AND IS_NULLABLE = 'NO' THEN '✓'
        WHEN COLUMN_NAME = 'CATEGORY_ID' AND DATA_TYPE = 'bigint' THEN '✓'
        WHEN COLUMN_NAME = 'CREATED_DATE' AND DATA_TYPE = 'timestamp' AND IS_NULLABLE = 'NO' THEN '✓'
        WHEN COLUMN_NAME = 'LAST_MODIFIED_DATE' AND DATA_TYPE = 'timestamp' THEN '✓'
        WHEN COLUMN_NAME = 'CREATED_BY' AND DATA_TYPE = 'bigint' AND IS_NULLABLE = 'NO' THEN '✓'
        WHEN COLUMN_NAME = 'LAST_MODIFIED_BY' AND DATA_TYPE = 'bigint' THEN '✓'
        ELSE '?'
    END AS 'Status'
FROM information_schema.columns
WHERE table_schema = DATABASE() AND table_name = 'PRODUCT'
ORDER BY ORDINAL_POSITION;

SELECT '=== KIỂM TRA CỘT BẢNG PRODUCT_DETAIL ===' AS 'CHECK';

SELECT 
    COLUMN_NAME AS 'Column',
    DATA_TYPE AS 'Type',
    IS_NULLABLE AS 'Nullable',
    COLUMN_DEFAULT AS 'Default',
    CASE 
        WHEN COLUMN_NAME = 'PRODUCT_ID' AND DATA_TYPE = 'bigint' AND COLUMN_KEY = 'PRI' THEN '✓'
        WHEN COLUMN_NAME = 'DESCRIPTION' AND DATA_TYPE = 'text' THEN '✓'
        WHEN COLUMN_NAME = 'CREATED_DATE' AND DATA_TYPE = 'timestamp' AND IS_NULLABLE = 'NO' THEN '✓'
        WHEN COLUMN_NAME = 'LAST_MODIFIED_DATE' AND DATA_TYPE = 'timestamp' THEN '✓'
        WHEN COLUMN_NAME = 'CREATED_BY' AND DATA_TYPE = 'bigint' AND IS_NULLABLE = 'NO' THEN '✓'
        WHEN COLUMN_NAME = 'LAST_MODIFIED_BY' AND DATA_TYPE = 'bigint' THEN '✓'
        ELSE '?'
    END AS 'Status'
FROM information_schema.columns
WHERE table_schema = DATABASE() AND table_name = 'PRODUCT_DETAIL'
ORDER BY ORDINAL_POSITION;

-- CẢNH BÁO: Domain model có Price nhưng database không có
SELECT 
    CASE 
        WHEN COUNT(*) = 0 THEN '⚠ CẢNH BÁO: Domain model ProductDetail có property Price nhưng database không có cột PRICE'
        ELSE '✓ Cột PRICE tồn tại'
    END AS 'Warning'
FROM information_schema.columns
WHERE table_schema = DATABASE() 
    AND table_name = 'PRODUCT_DETAIL' 
    AND column_name = 'PRICE';

SELECT '=== KIỂM TRA CỘT BẢNG ORDER ===' AS 'CHECK';

SELECT 
    COLUMN_NAME AS 'Column',
    DATA_TYPE AS 'Type',
    IS_NULLABLE AS 'Nullable',
    COLUMN_DEFAULT AS 'Default',
    CASE 
        WHEN COLUMN_NAME = 'ID' AND DATA_TYPE = 'bigint' AND COLUMN_KEY = 'PRI' THEN '✓'
        WHEN COLUMN_NAME = 'CUSTOMER_NAME' AND DATA_TYPE = 'varchar' AND IS_NULLABLE = 'NO' THEN '✓'
        WHEN COLUMN_NAME = 'CREATED_DATE' AND DATA_TYPE = 'timestamp' AND IS_NULLABLE = 'NO' THEN '✓'
        WHEN COLUMN_NAME = 'LAST_MODIFIED_DATE' AND DATA_TYPE = 'timestamp' THEN '✓'
        WHEN COLUMN_NAME = 'CREATED_BY' AND DATA_TYPE = 'bigint' AND IS_NULLABLE = 'NO' THEN '✓'
        WHEN COLUMN_NAME = 'LAST_MODIFIED_BY' AND DATA_TYPE = 'bigint' THEN '✓'
        ELSE '?'
    END AS 'Status'
FROM information_schema.columns
WHERE table_schema = DATABASE() AND table_name = 'ORDER'
ORDER BY ORDINAL_POSITION;

SELECT '=== KIỂM TRA CỘT BẢNG ORDER_ITEM ===' AS 'CHECK';

SELECT 
    COLUMN_NAME AS 'Column',
    DATA_TYPE AS 'Type',
    IS_NULLABLE AS 'Nullable',
    COLUMN_DEFAULT AS 'Default',
    CASE 
        WHEN COLUMN_NAME = 'ORDER_ID' AND DATA_TYPE = 'bigint' THEN '✓'
        WHEN COLUMN_NAME = 'PRODUCT_ID' AND DATA_TYPE = 'bigint' THEN '✓'
        WHEN COLUMN_NAME = 'QUANTITY' AND DATA_TYPE = 'int' THEN '✓'
        WHEN COLUMN_NAME = 'CREATED_DATE' AND DATA_TYPE = 'timestamp' AND IS_NULLABLE = 'NO' THEN '✓'
        WHEN COLUMN_NAME = 'LAST_MODIFIED_DATE' AND DATA_TYPE = 'timestamp' THEN '✓'
        WHEN COLUMN_NAME = 'CREATED_BY' AND DATA_TYPE = 'bigint' AND IS_NULLABLE = 'NO' THEN '✓'
        WHEN COLUMN_NAME = 'LAST_MODIFIED_BY' AND DATA_TYPE = 'bigint' THEN '✓'
        ELSE '?'
    END AS 'Status'
FROM information_schema.columns
WHERE table_schema = DATABASE() AND table_name = 'ORDER_ITEM'
ORDER BY ORDINAL_POSITION;

-- ============================================================================
-- 3. KIỂM TRA PRIMARY KEYS
-- ============================================================================

SELECT '=== KIỂM TRA PRIMARY KEYS ===' AS 'CHECK';

SELECT 
    TABLE_NAME AS 'Table',
    COLUMN_NAME AS 'Primary Key',
    CASE 
        WHEN TABLE_NAME = 'CATEGORY' AND COLUMN_NAME = 'ID' THEN '✓'
        WHEN TABLE_NAME = 'PRODUCT' AND COLUMN_NAME = 'ID' THEN '✓'
        WHEN TABLE_NAME = 'PRODUCT_DETAIL' AND COLUMN_NAME = 'PRODUCT_ID' THEN '✓'
        WHEN TABLE_NAME = 'ORDER' AND COLUMN_NAME = 'ID' THEN '✓'
        WHEN (TABLE_NAME = 'ORDER_ITEM' AND COLUMN_NAME IN ('ORDER_ID', 'PRODUCT_ID')) THEN '✓'
        ELSE '?'
    END AS 'Status'
FROM information_schema.key_column_usage
WHERE table_schema = DATABASE()
    AND constraint_name = 'PRIMARY'
    AND table_name IN ('CATEGORY', 'PRODUCT', 'PRODUCT_DETAIL', 'ORDER', 'ORDER_ITEM')
ORDER BY TABLE_NAME, ORDINAL_POSITION;

-- ============================================================================
-- 4. KIỂM TRA FOREIGN KEYS
-- ============================================================================

SELECT '=== KIỂM TRA FOREIGN KEYS ===' AS 'CHECK';

SELECT 
    CONSTRAINT_NAME AS 'FK Name',
    TABLE_NAME AS 'From Table',
    COLUMN_NAME AS 'Column',
    REFERENCED_TABLE_NAME AS 'To Table',
    REFERENCED_COLUMN_NAME AS 'Referenced Column',
    CASE 
        WHEN TABLE_NAME = 'PRODUCT' AND REFERENCED_TABLE_NAME = 'CATEGORY' AND COLUMN_NAME = 'CATEGORY_ID' THEN '✓'
        WHEN TABLE_NAME = 'PRODUCT_DETAIL' AND REFERENCED_TABLE_NAME = 'PRODUCT' AND COLUMN_NAME = 'PRODUCT_ID' THEN '✓'
        WHEN TABLE_NAME = 'ORDER_ITEM' AND REFERENCED_TABLE_NAME = 'ORDER' AND COLUMN_NAME = 'ORDER_ID' THEN '✓'
        WHEN TABLE_NAME = 'ORDER_ITEM' AND REFERENCED_TABLE_NAME = 'PRODUCT' AND COLUMN_NAME = 'PRODUCT_ID' THEN '✓'
        ELSE '?'
    END AS 'Status'
FROM information_schema.key_column_usage
WHERE table_schema = DATABASE()
    AND referenced_table_name IS NOT NULL
    AND table_name IN ('PRODUCT', 'PRODUCT_DETAIL', 'ORDER_ITEM')
ORDER BY TABLE_NAME, COLUMN_NAME;

-- ============================================================================
-- 5. KIỂM TRA INDEXES
-- ============================================================================

SELECT '=== KIỂM TRA INDEXES ===' AS 'CHECK';

SELECT 
    TABLE_NAME AS 'Table',
    INDEX_NAME AS 'Index',
    GROUP_CONCAT(COLUMN_NAME ORDER BY SEQ_IN_INDEX SEPARATOR ', ') AS 'Columns',
    CASE 
        WHEN INDEX_NAME = 'PRIMARY' THEN 'PK'
        WHEN INDEX_NAME = 'IX_PRODUCT_NAME' OR (TABLE_NAME = 'PRODUCT' AND COLUMN_NAME = 'NAME') THEN '✓'
        WHEN INDEX_NAME = 'IX_PRODUCT_CATEGORY_ID' OR (TABLE_NAME = 'PRODUCT' AND COLUMN_NAME = 'CATEGORY_ID' AND INDEX_NAME != 'PRIMARY') THEN '✓'
        WHEN INDEX_NAME = 'IX_CATEGORY_NAME' OR (TABLE_NAME = 'CATEGORY' AND COLUMN_NAME = 'NAME' AND INDEX_NAME != 'PRIMARY') THEN '✓'
        WHEN INDEX_NAME = 'IX_ORDER_CUSTOMER_NAME' OR (TABLE_NAME = 'ORDER' AND COLUMN_NAME = 'CUSTOMER_NAME' AND INDEX_NAME != 'PRIMARY') THEN '✓'
        WHEN INDEX_NAME = 'IX_ORDER_ITEM_ORDER_ID' OR (TABLE_NAME = 'ORDER_ITEM' AND COLUMN_NAME = 'ORDER_ID' AND INDEX_NAME != 'PRIMARY') THEN '✓'
        WHEN INDEX_NAME = 'IX_ORDER_ITEM_PRODUCT_ID' OR (TABLE_NAME = 'ORDER_ITEM' AND COLUMN_NAME = 'PRODUCT_ID' AND INDEX_NAME != 'PRIMARY') THEN '✓'
        ELSE '?'
    END AS 'Status'
FROM information_schema.statistics
WHERE table_schema = DATABASE()
    AND table_name IN ('CATEGORY', 'PRODUCT', 'PRODUCT_DETAIL', 'ORDER', 'ORDER_ITEM')
GROUP BY TABLE_NAME, INDEX_NAME
ORDER BY TABLE_NAME, INDEX_NAME;

-- ============================================================================
-- 6. KIỂM TRA DỮ LIỆU SAMPLE (Nếu có)
-- ============================================================================

SELECT '=== KIỂM TRA DỮ LIỆU SAMPLE ===' AS 'CHECK';

SELECT 
    'CATEGORY' AS 'Table',
    COUNT(*) AS 'Record Count'
FROM CATEGORY
UNION ALL
SELECT 
    'PRODUCT' AS 'Table',
    COUNT(*) AS 'Record Count'
FROM PRODUCT
UNION ALL
SELECT 
    'PRODUCT_DETAIL' AS 'Table',
    COUNT(*) AS 'Record Count'
FROM PRODUCT_DETAIL
UNION ALL
SELECT 
    'ORDER' AS 'Table',
    COUNT(*) AS 'Record Count'
FROM `ORDER`
UNION ALL
SELECT 
    'ORDER_ITEM' AS 'Table',
    COUNT(*) AS 'Record Count'
FROM ORDER_ITEM;

-- ============================================================================
-- 7. KIỂM TRA TÍNH TOÀN VẸN DỮ LIỆU (DATA INTEGRITY)
-- ============================================================================

SELECT '=== KIỂM TRA TÍNH TOÀN VẸN DỮ LIỆU ===' AS 'CHECK';

-- Kiểm tra PRODUCT có CATEGORY_ID không tồn tại trong CATEGORY
SELECT 
    CASE 
        WHEN COUNT(*) = 0 THEN '✓ Không có PRODUCT với CATEGORY_ID không hợp lệ'
        ELSE CONCAT('✗ Có ', COUNT(*), ' PRODUCT với CATEGORY_ID không tồn tại trong CATEGORY')
    END AS 'Status'
FROM PRODUCT p
LEFT JOIN CATEGORY c ON p.CATEGORY_ID = c.ID
WHERE p.CATEGORY_ID IS NOT NULL AND c.ID IS NULL;

-- Kiểm tra PRODUCT_DETAIL có PRODUCT_ID không tồn tại trong PRODUCT
SELECT 
    CASE 
        WHEN COUNT(*) = 0 THEN '✓ Không có PRODUCT_DETAIL với PRODUCT_ID không hợp lệ'
        ELSE CONCAT('✗ Có ', COUNT(*), ' PRODUCT_DETAIL với PRODUCT_ID không tồn tại trong PRODUCT')
    END AS 'Status'
FROM PRODUCT_DETAIL pd
LEFT JOIN PRODUCT p ON pd.PRODUCT_ID = p.ID
WHERE p.ID IS NULL;

-- Kiểm tra ORDER_ITEM có ORDER_ID không tồn tại trong ORDER
SELECT 
    CASE 
        WHEN COUNT(*) = 0 THEN '✓ Không có ORDER_ITEM với ORDER_ID không hợp lệ'
        ELSE CONCAT('✗ Có ', COUNT(*), ' ORDER_ITEM với ORDER_ID không tồn tại trong ORDER')
    END AS 'Status'
FROM ORDER_ITEM oi
LEFT JOIN `ORDER` o ON oi.ORDER_ID = o.ID
WHERE o.ID IS NULL;

-- Kiểm tra ORDER_ITEM có PRODUCT_ID không tồn tại trong PRODUCT
SELECT 
    CASE 
        WHEN COUNT(*) = 0 THEN '✓ Không có ORDER_ITEM với PRODUCT_ID không hợp lệ'
        ELSE CONCAT('✗ Có ', COUNT(*), ' ORDER_ITEM với PRODUCT_ID không tồn tại trong PRODUCT')
    END AS 'Status'
FROM ORDER_ITEM oi
LEFT JOIN PRODUCT p ON oi.PRODUCT_ID = p.ID
WHERE p.ID IS NULL;

-- Kiểm tra ORDER_ITEM có QUANTITY <= 0
SELECT 
    CASE 
        WHEN COUNT(*) = 0 THEN '✓ Không có ORDER_ITEM với QUANTITY không hợp lệ (<=0)'
        ELSE CONCAT('✗ Có ', COUNT(*), ' ORDER_ITEM với QUANTITY <= 0')
    END AS 'Status'
FROM ORDER_ITEM
WHERE QUANTITY <= 0;

-- ============================================================================
-- 8. TÓM TẮT KIỂM TRA
-- ============================================================================

SELECT '=== TÓM TẮT KIỂM TRA ===' AS 'SUMMARY';

SELECT 
    'Tổng số bảng' AS 'Item',
    COUNT(*) AS 'Count'
FROM information_schema.tables
WHERE table_schema = DATABASE()
    AND table_name IN ('CATEGORY', 'PRODUCT', 'PRODUCT_DETAIL', 'ORDER', 'ORDER_ITEM')
UNION ALL
SELECT 
    'Tổng số Foreign Keys' AS 'Item',
    COUNT(DISTINCT CONSTRAINT_NAME) AS 'Count'
FROM information_schema.key_column_usage
WHERE table_schema = DATABASE()
    AND referenced_table_name IS NOT NULL
    AND table_name IN ('PRODUCT', 'PRODUCT_DETAIL', 'ORDER_ITEM')
UNION ALL
SELECT 
    'Tổng số Indexes' AS 'Item',
    COUNT(DISTINCT INDEX_NAME) AS 'Count'
FROM information_schema.statistics
WHERE table_schema = DATABASE()
    AND table_name IN ('CATEGORY', 'PRODUCT', 'PRODUCT_DETAIL', 'ORDER', 'ORDER_ITEM');

-- ============================================================================
-- KẾT THÚC SCRIPT KIỂM TRA
-- ============================================================================