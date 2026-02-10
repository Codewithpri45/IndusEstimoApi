# Purchase Requisition Update - Sample JSON Request

## API Endpoint
```
POST /api/inventory/purchase-requisition/updateREQ
```

## Headers
```
Authorization: Bearer {your_jwt_token}
Content-Type: application/json
```

---

## Sample Request JSON for TransactionID 14

### Basic Update (Minimal Fields)
```json
{
  "Prefix": "PREQ",
  "TransactionID": 14,
  "RecordMain": [
    {
      "VoucherID": -9,
      "VoucherNo": "PREQ-0014",
      "LedgerID": 1,
      "VoucherDate": "2025-12-24T00:00:00",
      "TotalQuantity": 150.50,
      "Narration": "Updated Purchase Requisition for Raw Materials",
      "VoucherRemark": "Urgent requirement"
    }
  ],
  "RecordDetail": [
    {
      "ItemID": 101,
      "ItemName": "Raw Material A",
      "TransID": 1,
      "ItemGroupID": 5,
      "RequiredNoOfPacks": 10,
      "QuantityPerPack": 10,
      "RequiredQuantity": 100,
      "PurchaseQty": 100,
      "StockUnit": "KG",
      "OrderUnit": "KG",
      "ItemNarration": "High quality raw material",
      "ExpectedDeliveryDate": "2025-12-30T00:00:00",
      "RefJobBookingJobCardContentsID": 0,
      "RefJobCardContentNo": "",
      "CurrentStockInStockUnit": 50.5,
      "CurrentStockInPurchaseUnit": 50.5,
      "PhysicalStock": 50.5,
      "PhysicalStockInPurchaseUnit": 50.5,
      "IsAuditApproved": 0,
      "AuditApprovalRequired": 0
    }
  ],
  "UpdateIndentDetail": [],
  "UserApprovalProcess": [],
  "ObjvalidateLoginUser": {
    "UserName": "admin",
    "Password": "your_password"
  }
}
```

---

### Complete Update (With All Fields)
```json
{
  "Prefix": "PREQ",
  "TransactionID": 14,
  "RecordMain": [
    {
      "VoucherID": -9,
      "VoucherNo": "PREQ-0014",
      "LedgerID": 1,
      "VoucherDate": "2025-12-24T00:00:00",
      "TotalQuantity": 250.75,
      "Narration": "Updated Purchase Requisition - December 2025 Production Requirements",
      "VoucherRemark": "Urgent - Required for ongoing production schedule"
    }
  ],
  "RecordDetail": [
    {
      "ItemID": 101,
      "ItemName": "Paper Roll - 80 GSM",
      "TransID": 1,
      "ItemGroupID": 5,
      "RequiredNoOfPacks": 10,
      "QuantityPerPack": 10,
      "RequiredQuantity": 100,
      "PurchaseQty": 100,
      "StockUnit": "KG",
      "OrderUnit": "KG",
      "ItemNarration": "High quality paper roll for printing",
      "ExpectedDeliveryDate": "2025-12-30T00:00:00",
      "RefJobBookingJobCardContentsID": 5,
      "RefJobCardContentNo": "JOB-2025-001",
      "CurrentStockInStockUnit": 50.5,
      "CurrentStockInPurchaseUnit": 50.5,
      "PhysicalStock": 50.5,
      "PhysicalStockInPurchaseUnit": 50.5,
      "IsAuditApproved": 0,
      "AuditApprovalRequired": 0
    },
    {
      "ItemID": 102,
      "ItemName": "Adhesive - Premium Grade",
      "TransID": 2,
      "ItemGroupID": 5,
      "RequiredNoOfPacks": 5,
      "QuantityPerPack": 10,
      "RequiredQuantity": 50.5,
      "PurchaseQty": 50.5,
      "StockUnit": "KG",
      "OrderUnit": "KG",
      "ItemNarration": "Premium grade adhesive for lamination",
      "ExpectedDeliveryDate": "2025-12-31T00:00:00",
      "RefJobBookingJobCardContentsID": 5,
      "RefJobCardContentNo": "JOB-2025-001",
      "CurrentStockInStockUnit": 25.75,
      "CurrentStockInPurchaseUnit": 25.75,
      "PhysicalStock": 25.75,
      "PhysicalStockInPurchaseUnit": 25.75,
      "IsAuditApproved": 0,
      "AuditApprovalRequired": 0
    },
    {
      "ItemID": 103,
      "ItemName": "Ink - Cyan",
      "TransID": 3,
      "ItemGroupID": 6,
      "RequiredNoOfPacks": 20,
      "QuantityPerPack": 5,
      "RequiredQuantity": 100.25,
      "PurchaseQty": 100.25,
      "StockUnit": "LTR",
      "OrderUnit": "LTR",
      "ItemNarration": "Cyan ink for offset printing",
      "ExpectedDeliveryDate": "2025-12-28T00:00:00",
      "RefJobBookingJobCardContentsID": 6,
      "RefJobCardContentNo": "JOB-2025-002",
      "CurrentStockInStockUnit": 15.5,
      "CurrentStockInPurchaseUnit": 15.5,
      "PhysicalStock": 15.5,
      "PhysicalStockInPurchaseUnit": 15.5,
      "IsAuditApproved": 0,
      "AuditApprovalRequired": 0
    }
  ],
  "UpdateIndentDetail": [
    {
      "TransactionID": 10,
      "ItemID": 101,
      "JobBookingJobCardContentsID": 5,
      "RequisitionItemID": 101
    },
    {
      "TransactionID": 11,
      "ItemID": 102,
      "JobBookingJobCardContentsID": 5,
      "RequisitionItemID": 102
    },
    {
      "TransactionID": 12,
      "ItemID": 103,
      "JobBookingJobCardContentsID": 6,
      "RequisitionItemID": 103
    }
  ],
  "UserApprovalProcess": [
    {
      "ItemID": 101,
      "ItemName": "Paper Roll - 80 GSM",
      "PurchaseQty": 100
    },
    {
      "ItemID": 102,
      "ItemName": "Adhesive - Premium Grade",
      "PurchaseQty": 50.5
    },
    {
      "ItemID": 103,
      "ItemName": "Ink - Cyan",
      "PurchaseQty": 100.25
    }
  ],
  "ObjvalidateLoginUser": {
    "UserName": "admin",
    "Password": "Admin@123"
  }
}
```

---

## Field Descriptions

### RecordMain (Array with 1 object)
| Field | Type | Required | Description |
|-------|------|----------|-------------|
| VoucherID | long | Yes | Always -9 for Purchase Requisition |
| VoucherNo | string | Yes | Existing voucher number (e.g., "PREQ-0014") |
| LedgerID | long | Yes | Supplier/Vendor Ledger ID |
| VoucherDate | DateTime | Yes | Requisition date |
| TotalQuantity | decimal | Yes | Sum of all item quantities |
| Narration | string | No | Main narration/description |
| VoucherRemark | string | No | Additional remarks |

### RecordDetail (Array of items)
| Field | Type | Required | Description |
|-------|------|----------|-------------|
| ItemID | long | Yes | Item master ID |
| ItemName | string | Yes | Item name for reference |
| TransID | int | Yes | Sequential number (1, 2, 3...) |
| ItemGroupID | long | Yes | Item group ID |
| RequiredNoOfPacks | decimal | No | Number of packs |
| QuantityPerPack | decimal | No | Quantity per pack |
| RequiredQuantity | decimal | Yes | Total required quantity |
| PurchaseQty | decimal | Yes | Same as RequiredQuantity |
| StockUnit | string | Yes | Stock unit (KG, LTR, PCS, etc.) |
| OrderUnit | string | Yes | Order unit (usually same as StockUnit) |
| ItemNarration | string | No | Item-specific notes |
| ExpectedDeliveryDate | DateTime | No | Expected delivery date |
| RefJobBookingJobCardContentsID | long | No | Job card reference (0 if none) |
| RefJobCardContentNo | string | No | Job card number |
| CurrentStockInStockUnit | decimal | No | Current stock in stock unit |
| CurrentStockInPurchaseUnit | decimal | No | Current stock in purchase unit |
| PhysicalStock | decimal | No | Physical stock quantity |
| PhysicalStockInPurchaseUnit | decimal | No | Physical stock in purchase unit |
| IsAuditApproved | int | No | Audit approval status (0 or 1) |
| AuditApprovalRequired | int | No | Audit approval required (0 or 1) |

### UpdateIndentDetail (Array - Links to Indent transactions)
| Field | Type | Required | Description |
|-------|------|----------|-------------|
| TransactionID | long | Yes | Indent TransactionID to link |
| ItemID | long | Yes | Item ID |
| JobBookingJobCardContentsID | long | No | Job card contents ID (0 if none) |
| RequisitionItemID | long | Yes | Item ID in requisition |

### UserApprovalProcess (Array - For approval workflow)
| Field | Type | Required | Description |
|-------|------|----------|-------------|
| ItemID | long | Yes | Item ID |
| ItemName | string | Yes | Item name |
| PurchaseQty | decimal | Yes | Purchase quantity |

### ObjvalidateLoginUser (Authentication)
| Field | Type | Required | Description |
|-------|------|----------|-------------|
| UserName | string | Yes | Username for validation |
| Password | string | Yes | Password for validation |

---

## Expected Responses

### Success Response
```json
{
  "response": "Success",
  "TransactionID": "14",
  "VoucherNo": "PREQ-0014"
}
```

### Error Responses

**Transaction Used:**
```json
{
  "response": "TransactionUsed"
}
```

**Requisition Approved:**
```json
{
  "response": "RequisitionApproved"
}
```

**Update Failed:**
```json
{
  "response": "Error: Update failed"
}
```

**Invalid User:**
```json
{
  "response": "InvalidUser"
}
```

**Not Authorized:**
```json
{
  "response": "NotAuthorized"
}
```

---

## Testing with cURL

```bash
curl -X POST "https://your-api-url/api/inventory/purchase-requisition/updateREQ" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d @UpdateRequisition_Sample_Request.json
```

---

## Testing with Postman

1. **Method:** POST
2. **URL:** `{{baseUrl}}/api/inventory/purchase-requisition/updateREQ`
3. **Headers:**
   - `Authorization: Bearer {{token}}`
   - `Content-Type: application/json`
4. **Body:** Raw JSON (paste the sample JSON above)

---

## Important Notes

1. **TransactionID**: Must be an existing requisition ID (14 in this case)
2. **VoucherNo**: Should match the existing voucher number
3. **Authentication**: User credentials are validated before update
4. **Validation**: System checks if requisition is already used or approved
5. **Transaction Scope**: All updates happen in a database transaction
6. **Approval Workflow**: If configured, approval workflow is created for each item

---

## Common Scenarios

### Scenario 1: Update Quantities Only
```json
{
  "TransactionID": 14,
  "RecordMain": [{ /* existing main data */ }],
  "RecordDetail": [
    {
      "ItemID": 101,
      "RequiredQuantity": 150,  // Changed from 100
      "PurchaseQty": 150
      // ... other fields
    }
  ],
  "UpdateIndentDetail": [],
  "UserApprovalProcess": [],
  "ObjvalidateLoginUser": { /* credentials */ }
}
```

### Scenario 2: Add New Items
```json
{
  "TransactionID": 14,
  "RecordMain": [{ /* existing main data */ }],
  "RecordDetail": [
    { /* existing item 1 */ },
    { /* existing item 2 */ },
    {
      "ItemID": 104,  // New item
      "TransID": 3,
      "RequiredQuantity": 50,
      // ... other fields
    }
  ],
  "UpdateIndentDetail": [],
  "UserApprovalProcess": [],
  "ObjvalidateLoginUser": { /* credentials */ }
}
```

### Scenario 3: Remove Items
```json
{
  "TransactionID": 14,
  "RecordMain": [{ /* existing main data */ }],
  "RecordDetail": [
    { /* only item 1 - item 2 removed */ }
  ],
  "UpdateIndentDetail": [],
  "UserApprovalProcess": [],
  "ObjvalidateLoginUser": { /* credentials */ }
}
```

---

## Validation Rules

1. ✅ TransactionID must exist
2. ✅ Requisition must not be used in Purchase Order
3. ✅ Requisition must not be approved
4. ✅ User must have update permission
5. ✅ All required fields must be provided
6. ✅ TotalQuantity should match sum of item quantities
7. ✅ ItemID must exist in ItemMaster
8. ✅ Valid date formats (ISO 8601)

---

## Workflow

```
1. Validate user credentials
   ↓
2. Check if requisition is used
   ↓
3. Check if requisition is approved
   ↓
4. Validate production unit permission
   ↓
5. Update ItemTransactionMain
   ↓
6. Delete existing ItemTransactionDetail
   ↓
7. Insert new ItemTransactionDetail
   ↓
8. Reset linked indents
   ↓
9. Link new indents
   ↓
10. Create approval workflow (if needed)
    ↓
11. Return success with TransactionID
```
