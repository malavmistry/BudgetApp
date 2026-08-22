const RecurringItemModule = (() => {
    let _modal = null;
    let _items = [];
    let _categories = [];

    function getModal() {
        if (!_modal) {
            _modal = new bootstrap.Modal(document.getElementById("recurringItemModal"));
        }

        return _modal;
    }

    async function openRecurringItemList() {
        getModal().show();
        await renderRecurringItemList();
    }

    async function renderRecurringItemList() {
        const body = document.getElementById("recurringItemModalBody");
        body.innerHTML = spinner();

        const [items, categories] = await Promise.all([
            SiteUtils.getJson("/RecurringItemApi?handler=List"),
            SiteUtils.getJson("/BudgetApi?handler=Categories")
        ]);

        _items = items || [];
        _categories = categories || [];

        body.innerHTML = `
            <div class="d-flex justify-content-end mb-3">
                <button class="btn btn-primary btn-sm" onclick="RecurringItemModule.openEditForm(0)">
                    <i class="bi bi-plus-lg me-1"></i>Add Recurring Item
                </button>
            </div>
            <div id="recurringItemFormContainer"></div>
            <div class="table-responsive">
                <table class="table table-hover table-sm">
                    <thead class="table-light">
                        <tr>
                            <th>Name</th>
                            <th>Type</th>
                            <th>Category</th>
                            <th>Day</th>
                            <th class="text-end">Amount</th>
                            <th>Status</th>
                            <th></th>
                        </tr>
                    </thead>
                    <tbody>${_items.map(renderRow).join("")}</tbody>
                </table>
                ${_items.length === 0 ? "<p class=\"text-muted text-center py-3\">No recurring items yet.</p>" : ""}
            </div>`;
    }

    function renderRow(item) {
        return `<tr>
            <td class="fw-semibold small">${escHtml(item.itemNameText)}</td>
            <td><span class="badge ${item.type === 2 ? "bg-success-subtle text-success" : "bg-danger-subtle text-danger"} small">${item.type === 2 ? "Earnings" : "Expense"}</span></td>
            <td class="small text-muted">${escHtml(item.categoryName || "")}</td>
            <td class="small">${item.dayOfMonth}</td>
            <td class="text-end fw-semibold small">$${fmt(item.amount)}</td>
            <td><span class="badge ${item.isActive ? "bg-success-subtle text-success" : "bg-secondary-subtle text-secondary"} small">${item.isActive ? "Active" : "Inactive"}</span></td>
            <td>
                <button class="btn btn-outline-primary btn-sm" onclick="RecurringItemModule.openEditForm(${item.id})">
                    <i class="bi bi-pencil"></i>
                </button>
            </td>
        </tr>`;
    }

    function openEditForm(id) {
        const item = id === 0 ? null : _items.find(x => x.id === id);
        const categoryOptions = _categories.map(c =>
            `<option value="${c.id}" ${item && item.categoryId === c.id ? "selected" : ""}>${escHtml(c.name)}</option>`
        ).join("");

        document.getElementById("recurringItemFormContainer").innerHTML = `
            <div class="card border-primary mb-3">
                <div class="card-header bg-primary-subtle fw-semibold">${id === 0 ? "Add Recurring Item" : "Edit Recurring Item"}</div>
                <div class="card-body">
                    <form onsubmit="RecurringItemModule.saveRecurringItem(event, ${id})">
                        <div class="row g-2 mb-2">
                            <div class="col-sm-3">
                                <label class="form-label small fw-semibold">Type <span class="text-danger">*</span></label>
                                <select class="form-select form-select-sm" id="riType" required>
                                    <option value="1" ${item && item.type === 1 ? "selected" : ""}>Expense</option>
                                    <option value="2" ${item && item.type === 2 ? "selected" : ""}>Earnings</option>
                                </select>
                            </div>
                            <div class="col-sm-4 position-relative">
                                <label class="form-label small fw-semibold">Name <span class="text-danger">*</span></label>
                                <input type="text" class="form-control form-control-sm" id="riName"
                                       maxlength="25" value="${item ? escHtml(item.itemNameText) : ""}"
                                       autocomplete="off" oninput="RecurringItemModule.searchItemNames(this.value)"
                                       onblur="setTimeout(()=>document.getElementById('riNameDropdown').innerHTML='',200)"
                                       required />
                                <div id="riNameDropdown" class="autocomplete-list"></div>
                            </div>
                            <div class="col-sm-3">
                                <label class="form-label small fw-semibold">Category <span class="text-danger">*</span></label>
                                <select class="form-select form-select-sm" id="riCategory" required>
                                    <option value="">Select...</option>
                                    ${categoryOptions}
                                </select>
                            </div>
                            <div class="col-sm-2">
                                <label class="form-label small fw-semibold">Amount <span class="text-danger">*</span></label>
                                <input type="number" class="form-control form-control-sm" id="riAmount"
                                       step="0.01" min="0.01" value="${item ? item.amount : ""}" required />
                            </div>
                        </div>
                        <div class="row g-2 mb-3">
                            <div class="col-sm-3">
                                <label class="form-label small fw-semibold">Day of Month <span class="text-danger">*</span></label>
                                <input type="number" class="form-control form-control-sm" id="riDayOfMonth"
                                       min="1" max="31" value="${item ? item.dayOfMonth : new Date().getDate()}" required />
                            </div>
                            <div class="col-sm-6">
                                <label class="form-label small fw-semibold">Note</label>
                                <input type="text" class="form-control form-control-sm" id="riNote"
                                       maxlength="500" value="${item ? escHtml(item.note || "") : ""}" />
                            </div>
                            <div class="col-sm-3 d-flex align-items-end">
                                <div class="form-check">
                                    <input type="checkbox" class="form-check-input" id="riIsActive"
                                           ${item ? (item.isActive ? "checked" : "") : "checked"} />
                                    <label class="form-check-label small" for="riIsActive">Active</label>
                                </div>
                            </div>
                        </div>
                        <div class="d-flex gap-2">
                            <button type="submit" class="btn btn-primary btn-sm"><i class="bi bi-save me-1"></i>Save</button>
                            <button type="button" class="btn btn-secondary btn-sm" onclick="document.getElementById('recurringItemFormContainer').innerHTML=''">Cancel</button>
                        </div>
                    </form>
                </div>
            </div>`;
    }

    async function searchItemNames(query) {
        const dropdown = document.getElementById("riNameDropdown");
        if (!query || query.length < 1) {
            dropdown.innerHTML = "";
            return;
        }

        const results = await SiteUtils.getJson(`/ItemNameApi?handler=Search&q=${encodeURIComponent(query)}`);
        if (!results || !results.length) {
            dropdown.innerHTML = "";
            return;
        }

        dropdown.innerHTML = results.map(r =>
            `<div class="ac-item" onmousedown="RecurringItemModule.selectItemName('${escHtml(r.name)}')">${escHtml(r.name)}</div>`
        ).join("");
    }

    function selectItemName(name) {
        document.getElementById("riName").value = name;
        document.getElementById("riNameDropdown").innerHTML = "";
    }

    async function saveRecurringItem(evt, id) {
        evt.preventDefault();

        const payload = {
            id,
            type: parseInt(document.getElementById("riType").value),
            itemNameId: 0,
            itemNameText: document.getElementById("riName").value.trim(),
            categoryId: parseInt(document.getElementById("riCategory").value),
            amount: parseFloat(document.getElementById("riAmount").value),
            note: document.getElementById("riNote").value.trim() || null,
            dayOfMonth: parseInt(document.getElementById("riDayOfMonth").value),
            isActive: document.getElementById("riIsActive").checked
        };

        const result = await SiteUtils.postJson("/RecurringItemApi?handler=Save", payload);
        if (result.success) {
            SiteUtils.showToast("Recurring item saved.");
            await renderRecurringItemList();
        } else {
            SiteUtils.showToast("Failed to save recurring item.", true);
        }
    }

    function spinner() {
        return "<div class=\"text-center py-4\"><div class=\"spinner-border text-primary\" role=\"status\"></div></div>";
    }

    function fmt(n) {
        return parseFloat(n || 0).toFixed(2).replace(/\B(?=(\d{3})+(?!\d))/g, ",");
    }

    function escHtml(text) {
        const d = document.createElement("div");
        d.textContent = String(text || "");
        return d.innerHTML;
    }

    return {
        openRecurringItemList,
        openEditForm,
        searchItemNames,
        selectItemName,
        saveRecurringItem
    };
})();