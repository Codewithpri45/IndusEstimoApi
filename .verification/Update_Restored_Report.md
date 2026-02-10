# Feature Restoration: Purchase Requisition Update

## ✅ Overview
The Update functionality for Purchase Requisition has been fully restored and integrated alongside the Save functionality, following the legacy system's logic flow but using modern C# architecture.

## 🔄 Restoration Architecture

### 1. API Endpoint
*   **Endpoint:** `POST /api/inventory/purchase-requisition/updateREQ`
*   **Behavior:** Delegates processing to `SavePurchaseRequisitionAsync` in the Service layer, maintaining a unified entry point for "Upsert" logic at the service level while exposing distinct API endpoints for clarity.

### 2. Service Logic (`PurchaseRequisitionService.cs`)
The `SavePurchaseRequisitionAsync` method has been enhanced to handle both **Insert** and **Update** scenarios:
*   **Checks:** 
    *   If `TransactionID > 0`, it triggers Update logic.
    *   Validates if transaction is "Used" or "Approved" (returns error if so).
    *   Checks "Update" permission vs "Save" permission dynamically.
*   **Execution:** Calls `_repository.UpdatePurchaseRequisitionAsync` for updates.

### 3. Repository Logic (`PurchaseRequisitionRepository.cs`)
A robust `UpdatePurchaseRequisitionAsync` method has been implemented:
*   **Flow:**
    1.  **Update Main:** Updates `ItemTransactionMain` using dynamic SQL generation (`UpdateDataAsync`), ensuring audit fields (`ModifiedBy`, `ModifiedDate`) are set.
    2.  **Delete Details:** Deletes existing records in `ItemTransactionDetail` for the transaction.
    3.  **Insert Details:** Inserts new records from the request into `ItemTransactionDetail`.
    4.  **Reset Indents:** Clears linkage for previously associated Indents.
    5.  **Update Indents:** Links current Indents to the transaction.
    6.  **Transaction:** All operations run within a single `SqlTransaction`.

### 4. Dynamic Query Generation (`DbOperationsService`)
*   Used `UpdateDataAsync` and `InsertDataAsync` which rely on Dapper and Reflection to dynamically generate SQL based on object properties, fulfilling the requirement for a dynamic update function similar to the legacy `UpdateDatatableToDatabase`.

## 🧪 Verification

### Build Status
**Status:** ✅ SUCCESS
**Exit Code:** 0

### Functionality Checklist
- [x] `updateREQ` endpoint matches legacy signature? **Yes** (Input JSON structure).
- [x] Validations (Used/Approved) restored? **Yes**.
- [x] Permission check (Save vs Update) restored? **Yes**.
- [x] Database atomicity (TransactionScope)? **Yes** (via `SqlTransaction`).
- [x] Audit fields populated? **Yes**.
- [x] **Bug Fix:** Resolved `TargetParameterCountException` by explicitly casting connection/transaction to concrete types to invoke the correct list-handling overload of `InsertDataAsync`.

## 📝 Usage

Post to `updateREQ` with a JSON body containing `TransactionID > 0`. The system will automatically detect the ID and perform an Update instead of a Save.
