# Feature Restoration Status

## 🔄 Update: Purchase Requisition Update Restored

As of the latest session, the **Update functionality has been fully restored** and reintegrated into the application.

### 📋 Current Status

1.  **Controller Layer:**
    *   Added `[HttpPost("updateREQ")]` endpoint which routes to the Save logic.
    *   Both `saveREQ` and `updateREQ` are functional.

2.  **Service Layer:**
    *   `SavePurchaseRequisitionAsync` now strictly handles "Upsert" logic.
    *   If `TransactionID > 0`, it executes the Update flow.
    *   If `TransactionID == 0`, it executes the Save flow.
    *   Validations for "Used" and "Approved" transactions are active.

3.  **Repository Layer:**
    *   Implemented `UpdatePurchaseRequisitionAsync` supporting:
        *   Main record update (using dynamic query generation).
        *   Detail record replacement (Delete + Insert).
        *   Indent linkage update.
    *   Used `SqlTransaction` for data integrity.

4.  **Bug Fix:**
    *   Resolved `TargetParameterCountException` by fixing overload resolution for `InsertDataAsync` with List types.

## 🔗 Reference
See `Update_Restored_Report.md` for detailed verification and usage instructions.
