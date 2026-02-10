# API Review: SavePurchaseRequisition (saveREQ)

## ✅ Overview
The `saveREQ` API endpoint has been reviewed following the removal of the update functionality. It is correctly designed to handle the creation of **new** Purchase Requisitions only.

---

## 🔍 Layer-by-Layer Analysis

### 1. Controller Layer
**File:** `PurchaseRequisitionController.cs`
- **Endpoint:** `[HttpPost("saveREQ")]`
- **Logic:** Delegates entirely to `_service.SavePurchaseRequisitionAsync(request)`.
- **Response Handling:**
  - **Success:** Returns JSON with `response: "Success"`, `TransactionID`, and `VoucherNo`.
  - **Failure:** Returns JSON with `response: {ErrorMessage}`.
- **Verdict:** ✅ Correctly implemented.

### 2. Service Layer
**File:** `PurchaseRequisitionService.cs`
- **Method:** `SavePurchaseRequisitionAsync`
- **Validations:**
  - Authenticates user context.
  - Validates "Save" permission against `ValidationProductionUnitAsync`.
- **Core Logic:**
  - Generates new Voucher Number via `GenerateNextPRNumberAsync`.
  - Maps DTOs (Main, Detail, IndentUpdates) to Entities with proper audit fields.
  - Calls Repository to save data in a transaction.
  - Triggers `CreateApprovalWorkflowAsync` (non-blocking) if configured.
- **Cleanup:** Update logic has been successfully removed.
- **Verdict:** ✅ Correctly implemented.

### 3. Repository Layer
**File:** `PurchaseRequisitionRepository.cs`
- **Method:** `SavePurchaseRequisitionAsync`
- **Transaction:** Uses `SqlTransaction` to ensure atomicity (all-or-nothing).
- **Operations:**
  - Inserts into `ItemTransactionMain`.
  - Inserts into `ItemTransactionDetail` (linked to Main).
  - Updates Indent links via `UpdateIndentDetailsAsync`.
- **Note:** The stored procedure `UPDATE_ITEM_STOCK_VALUES_UNIT_WISE` is intentionally omitted as it does not exist in the database.
- **Verdict:** ✅ Correctly implemented.

---

## 🧪 Testing Recommendations

Since the API is now dedicated to saving only, test scenarios should focus on creation:

1.  **Basic Save:** Create a PR with 1 main record and multiple details.
    *   *Expect:* New `TransactionID` and `VoucherNo`.
2.  **With Indents:** Create a PR linked to Indent items (`UpdateIndentDetail` populated).
    *   *Expect:* `ItemTransactionDetail` table should reflect correct `RequisitionTransactionID`.
3.  **Validation:** Try with a user who lacks permission.
    *   *Expect:* Error response.
4.  **Approval Workflow:** Create a PR that triggers approval (if configured).
    *   *Expect:* Workflow entries created without blocking the save.

## 🏁 Conclusion
The `saveREQ` API is **READY** for use. It is clean, focused on creation, and follows the expected patterns of the legacy system.
