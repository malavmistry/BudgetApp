/* budget.js — Budget list and item management */

const BudgetModule = (() => {
    let _budgetModal = null;
    let _editModal = null;
    let _addItemModal = null;
    let _allBudgets = [];
    let _categories = [];
    let _currentBudgetId = null;
    let _currentItems = [];
    let _currentBudget = null;

    function getBudgetModal() {
        if (!_budgetModal) _budgetModal = new bootstrap.Modal(document.getElementById("budgetModal"));
        return _budgetModal;
    }

    function getEditModal() {
        if (!_editModal) _editModal = new bootstrap.Modal(document.getElementById("budgetEditModal"));
        return _editModal;
    }

    function getAddItemModal() {
        if (!_addItemModal) _addItemModal = new bootstrap.Modal(document.getElementById("addItemModal"));
        return _addItemModal;
    }

    async function openBudgetList() {
        getBudgetModal().show();
        await renderBudgetList();
    }

    async function renderBudgetList() {
        const body = document.getElementById("budgetModalBody");
        body.innerHTML = spinner();

        const budgets = await SiteUtils.getJson("/BudgetApi?handler=List");
        _allBudgets = budgets;

        const rows = budgets.map(b => `
            <tr>
                <td class="fw-semibold small">${escHtml(b.name)}</td>
                <td>
                    <span class="badge ${b.isTimeBound ? "bg-primary-subtle text-primary" : "bg-secondary-subtle text-secondary"} small">
                        ${b.isTimeBound ? "Monthly" : "Custom"}
                    </span>
                </td>
                <td>
                    <div class="d-flex gap-2">
                        <button class="btn btn-sm btn-outline-primary" onclick="BudgetModule.openBudgetEdit(${b.id})">
                            <i class="bi bi-journal-text"></i>
                        </button>
                    </div>
                </td>
            </tr>`).join("");

        body.innerHTML = `
            <div class="d-flex justify-content-end mb-3">
                <button class="btn btn-primary btn-sm" onclick="BudgetModule.showCreateForm()">
                    <i class="bi bi-plus-lg me-1"></i>New Budget
                </button>
            </div>
            <div id="budgetCreateFormContainer"></div>
            <div class="table-responsive">
                <table class="table table-hover table-sm">
                    <thead class="table-light">
                        <tr><th>Name</th><th>Type</th><th></th></tr>
                    </thead>
                    <tbody>${rows}</tbody>
                </table>
                ${budgets.length === 0 ? "<p class=\"text-muted text-center py-3\">No budgets yet.</p>" : ""}
            </div>`;
    }

    function showCreateForm() {
        const html = `
            <div class="card border-primary mb-3">
                <div class="card-header bg-primary-subtle fw-semibold">New Budget</div>
                <div class="card-body">
                    <form onsubmit="BudgetModule.createBudget(event)">
                        <div class="mb-3">
                            <label class="form-label fw-semibold">Name <span class="text-danger">*</span></label>
                            <input type="text" class="form-control" id="newBudgetName" maxlength="25" required />
                        </div>
                        <div class="mb-3 form-check">
                            <input type="checkbox" class="form-check-input" id="newBudgetTimeBound"
                                   onchange="document.getElementById('monthYearFields').classList.toggle('d-none', !this.checked)" />
                            <label class="form-check-label" for="newBudgetTimeBound">Time-bound (monthly)</label>
                        </div>
                        <div id="monthYearFields" class="d-none row g-2 mb-3">
                            <div class="col-6">
                                <label class="form-label small fw-semibold">Month</label>
                                <select class="form-select form-select-sm" id="newBudgetMonth">
                                    ${Array.from({length:12},(_,i) => `<option value="${i+1}">${new Date(2000,i).toLocaleString("default",{month:"long"})}</option>`).join("")}
                                </select>
                            </div>
                            <div class="col-6">
                                <label class="form-label small fw-semibold">Year (2-digit)</label>
                                <input type="number" class="form-control form-control-sm" id="newBudgetYear"
                                       min="0" max="99" value="${new Date().getFullYear() % 100}" />
                            </div>
                        </div>
                        <div class="d-flex gap-2">
                            <button type="submit" class="btn btn-primary btn-sm"><i class="bi bi-save me-1"></i>Create</button>
                            <button type="button" class="btn btn-secondary btn-sm"
                                    onclick="document.getElementById('budgetCreateFormContainer').innerHTML=''">Cancel</button>
                        </div>
                    </form>
                </div>
            </div>`;
        document.getElementById("budgetCreateFormContainer").innerHTML = html;
    }

    async function createBudget(evt) {
        evt.preventDefault();
        const isTimeBound = document.getElementById("newBudgetTimeBound").checked;
        const payload = {
            name: document.getElementById("newBudgetName").value.trim(),
            isTimeBound,
            month: isTimeBound ? parseInt(document.getElementById("newBudgetMonth").value) : null,
            year: isTimeBound ? parseInt(document.getElementById("newBudgetYear").value) : null
        };
        const result = await SiteUtils.postJson("/BudgetApi?handler=CreateBudget", payload);
        if (result.success) {
            SiteUtils.showToast(`Budget "${result.name}" created.`);
            await renderBudgetList();
        } else {
            SiteUtils.showToast("Failed to create budget.", true);
        }
    }

    async function openBudgetEdit(budgetId) {
        _currentBudgetId = budgetId;
        getBudgetModal().hide();
        getEditModal().show();
        await renderBudgetEdit(budgetId);
    }

    async function renderBudgetEdit(budgetId) {
        const body = document.getElementById("budgetEditModalBody");
        body.innerHTML = spinner();

        const [budget, categories, allBudgets] = await Promise.all([
            SiteUtils.getJson(`/BudgetApi?handler=Detail&budgetId=${budgetId}`),
            SiteUtils.getJson("/BudgetApi?handler=Categories"),
            SiteUtils.getJson("/BudgetApi?handler=AllBudgets")
        ]);

        _categories = categories;
        _allBudgets = allBudgets;

        const items = budget.items || [];
        _currentItems = items;
        _currentBudget = budget;
        const earnings = items.filter(i => i.type === 2).reduce((s, i) => s + i.amount, 0);
        const expense = items.filter(i => i.type === 1).reduce((s, i) => s + i.amount, 0);
        const net = earnings - expense;

        const itemRows = items.map(renderItemRow).join("");

        document.getElementById("budgetEditModalLabel").innerHTML =
            "<span id='budgetNameDisplay'>" + escHtml(budget.name) + "</span>" +
            " <button class='btn btn-sm btn-outline-light ms-2 p-0 px-1' onclick='BudgetModule.startEditBudgetName(" + budgetId + ")' title='Rename budget'>" +
            "<i class='bi bi-pencil-square'></i></button>";

        body.innerHTML = `
            <div class="row g-3 mb-3">
                <div class="col-sm-4">
                    <div class="stat-card bg-success-subtle rounded p-3">
                        <div class="small text-muted fw-semibold">EARNINGS</div>
                        <div class="fs-5 fw-bold text-success">$${fmt(earnings)}</div>
                    </div>
                </div>
                <div class="col-sm-4">
                    <div class="stat-card bg-danger-subtle rounded p-3">
                        <div class="small text-muted fw-semibold">EXPENSE</div>
                        <div class="fs-5 fw-bold text-danger">$${fmt(expense)}</div>
                    </div>
                </div>
                <div class="col-sm-4">
                    <div class="stat-card ${net >= 0 ? "bg-primary-subtle" : "bg-warning-subtle"} rounded p-3">
                        <div class="small text-muted fw-semibold">NET</div>
                        <div class="fs-5 fw-bold ${net >= 0 ? "text-primary" : "text-warning-emphasis"}">$${fmt(net)}</div>
                    </div>
                </div>
            </div>

            <div class="d-flex justify-content-end mb-2">
                <button class="btn btn-primary btn-sm" onclick="BudgetModule.showItemForm(0, ${budgetId})">
                    <i class="bi bi-plus-lg me-1"></i>Add Item
                </button>
            </div>
            <div id="itemFormContainer"></div>

            <div class="table-responsive">
                <table class="table table-hover table-sm" id="itemsTable">
                    <thead class="table-light">
                        <tr>
                            <th>Date</th>
                            <th>Type</th>
                            <th>Name</th>
                            <th>Category</th>
                            <th class="text-end">Amount</th>
                            <th>Note</th>
                            <th>Links</th>
                            <th></th>
                        </tr>
                    </thead>
                    <tbody id="itemsTableBody">${itemRows}</tbody>
                </table>
                ${items.length === 0 ? "<p class=\"text-muted text-center py-3\">No items yet.</p>" : ""}
            </div>`;
    }

    function renderItemRow(item) {
        const typeLabel = item.type === 2 ? "Earnings" : "Expense";
        const typeClass = item.type === 2 ? "bg-success-subtle text-success" : "bg-danger-subtle text-danger";
        const amtClass = item.type === 2 ? "text-success" : "text-danger";
        const recurringIcon = item.isRecurring
            ? "<i class='bi bi-arrow-repeat text-success ms-1' title='Recurring'></i>"
            : "";
        const linkedNames = item.linkedBudgetNames || [];
        const linkIndicator = linkedNames.length > 0
            ? `<span class="badge bg-info-subtle text-info link-indicator"
                    onclick="BudgetModule.showLinkedBudgets(${item.id})"
                    title="Included in ${linkedNames.length} more budget(s)">
                    <i class="bi bi-link-45deg"></i>${linkedNames.length}
               </span>`
            : "";

        return `<tr class="budget-item-row" data-id="${item.id}">
            <td class="text-nowrap small">${escHtml(item.transactionDate)}</td>
            <td><span class="badge ${typeClass} small">${typeLabel}</span></td>
            <td class="small fw-semibold">${escHtml(item.itemNameText)}${recurringIcon}</td>
            <td class="small text-muted">${escHtml(item.categoryName)}</td>
            <td class="text-end fw-semibold small ${amtClass}">$${fmt(item.amount)}</td>
            <td class="small text-muted">${escHtml(item.note || "")}</td>
            <td>${linkIndicator}</td>
            <td>
                <div class="d-flex gap-1">
                    <button class="btn btn-sm btn-outline-secondary"
                            onclick="BudgetModule.showItemForm(${item.id}, ${item.budgetId})">
                        <i class="bi bi-pencil"></i>
                    </button>
                    <button class="btn btn-sm btn-outline-danger"
                            onclick="BudgetModule.deleteItem(${item.id})">
                        <i class="bi bi-trash"></i>
                    </button>
                </div>
            </td>
        </tr>`;
    }

    function showItemForm(itemId, budgetId) {
        const item = itemId === 0 ? null : _findItem(itemId);

        // Compute date constraints and default for time-bound budgets
        let dateMin = "";
        let dateMax = "";
        let defaultDateInput = toDateInput(new Date().toLocaleDateString("en-US", {
            month: "2-digit", day: "2-digit", year: "numeric"
        }).replace(/\//g, "-"));

        if (_currentBudget && _currentBudget.isTimeBound && _currentBudget.month && _currentBudget.year) {
            const fullYear = 2000 + _currentBudget.year;
            const month = String(_currentBudget.month).padStart(2, "0");
            const lastDay = new Date(fullYear, _currentBudget.month, 0).getDate();
            dateMin = `${fullYear}-${month}-01`;
            dateMax = `${fullYear}-${month}-${String(lastDay).padStart(2, "0")}`;

            // Default: today if in this month, otherwise 1st of the budget month
            const todayObj = new Date();
            const inRange = todayObj.getFullYear() === fullYear && (todayObj.getMonth() + 1) === _currentBudget.month;
            defaultDateInput = inRange ? defaultDateInput : dateMin;
        }

        const catOptions = _categories.map(c =>
            `<option value="${c.id}" ${item && item.categoryId === c.id ? "selected" : ""}>${escHtml(c.name)}</option>`
        ).join("");

        const budgetCheckboxes = _allBudgets
            .filter(b => b.id !== budgetId)
            .map(b => {
                const linked = item && item.linkedBudgetIds && item.linkedBudgetIds.includes(b.id);
                return `<div class="form-check">
                    <input class="form-check-input" type="checkbox" id="link_${b.id}" value="${b.id}" ${linked ? "checked" : ""} />
                    <label class="form-check-label small" for="link_${b.id}">${escHtml(b.name)}</label>
                </div>`;
            }).join("");

        const html = `
            <div class="card border-primary mb-3">
                <div class="card-header bg-primary-subtle fw-semibold">${itemId === 0 ? "Add Item" : "Edit Item"}</div>
                <div class="card-body">
                    <form onsubmit="BudgetModule.saveItem(event, ${itemId}, ${budgetId})">
                        <div class="row g-2 mb-2">
                            <div class="col-sm-3">
                                <label class="form-label small fw-semibold">Type <span class="text-danger">*</span></label>
                                <select class="form-select form-select-sm" id="itemType" required>
                                    <option value="1" ${item && item.type === 1 ? "selected" : ""}>Expense</option>
                                    <option value="2" ${item && item.type === 2 ? "selected" : ""}>Earnings</option>
                                </select>
                            </div>
                            <div class="col-sm-4 position-relative">
                                <label class="form-label small fw-semibold">Name <span class="text-danger">*</span></label>
                                <input type="text" class="form-control form-control-sm" id="itemNameInput"
                                       maxlength="25" value="${item ? escHtml(item.itemNameText) : ""}"
                                       autocomplete="off" oninput="BudgetModule.searchItemNames(this.value)"
                                       onblur="setTimeout(()=>document.getElementById('itemNameDropdown').innerHTML='',200)"
                                       required />
                                <div id="itemNameDropdown" class="autocomplete-list"></div>
                            </div>
                            <div class="col-sm-3">
                                <label class="form-label small fw-semibold">Category <span class="text-danger">*</span></label>
                                <select class="form-select form-select-sm" id="itemCategory" required>
                                    <option value="">Select...</option>
                                    ${catOptions}
                                </select>
                            </div>
                            <div class="col-sm-2">
                                <label class="form-label small fw-semibold">Amount <span class="text-danger">*</span></label>
                                <input type="number" class="form-control form-control-sm" id="itemAmount"
                                       step="0.01" min="0.01" value="${item ? item.amount : ""}" required />
                            </div>
                        </div>
                        <div class="row g-2 mb-2">
                            <div class="col-sm-3">
                                <label class="form-label small fw-semibold">Date <span class="text-danger">*</span></label>
                                <input type="date" class="form-control form-control-sm" id="itemDate"
                                       value="${item ? toDateInput(item.transactionDate) : defaultDateInput}"
                                       min="${dateMin}" max="${dateMax}" required />
                            </div>
                            <div class="col-sm-6">
                                <label class="form-label small fw-semibold">Note</label>
                                <input type="text" class="form-control form-control-sm" id="itemNote"
                                       maxlength="500" value="${item ? escHtml(item.note || "") : ""}" />
                            </div>
                            <div class="col-sm-3">
                                <label class="form-label small fw-semibold">Include In</label>
                                <div id="itemLinkedBudgets" class="border rounded p-2 bg-white" style="max-height:120px;overflow-y:auto;">
                                    ${budgetCheckboxes || "<span class='text-muted small'>No other budgets</span>"}
                                </div>
                            </div>
                        </div>
                        <div class="d-flex gap-2 mt-2">
                            <button type="submit" class="btn btn-primary btn-sm"><i class="bi bi-save me-1"></i>Save</button>
                            <button type="button" class="btn btn-secondary btn-sm"
                                    onclick="document.getElementById('itemFormContainer').innerHTML=''">Cancel</button>
                        </div>
                        <div class="form-check mt-2">
                            <input class="form-check-input" type="checkbox" id="itemRecurring"
                                   ${item && item.isRecurring ? "checked" : ""} />
                            <label class="form-check-label small" for="itemRecurring">
                                <i class="bi bi-arrow-repeat me-1 text-success"></i>
                                <strong>Recurring</strong> — automatically copy to new monthly budgets
                            </label>
                        </div>
                    </form>
                </div>
            </div>`;

        document.getElementById("itemFormContainer").innerHTML = html;
    }

    async function searchItemNames(query) {
        const dropdown = document.getElementById("itemNameDropdown");
        if (!query || query.length < 1) { dropdown.innerHTML = ""; return; }

        const results = await SiteUtils.getJson(`/ItemNameApi?handler=Search&q=${encodeURIComponent(query)}`);
        if (!results || !results.length) { dropdown.innerHTML = ""; return; }

        dropdown.innerHTML = results.map(r =>
            `<div class="ac-item" onmousedown="BudgetModule.selectItemName('${escHtml(r.name)}')">${escHtml(r.name)}</div>`
        ).join("");
    }

    function selectItemName(name) {
        document.getElementById("itemNameInput").value = name;
        document.getElementById("itemNameDropdown").innerHTML = "";
    }

    async function saveItem(evt, itemId, budgetId) {
        evt.preventDefault();

        const linkedIds = Array.from(document.querySelectorAll("#itemLinkedBudgets input[type=checkbox]:checked"))
            .map(cb => parseInt(cb.value));

        const payload = {
            id: itemId,
            budgetId: budgetId,
            type: parseInt(document.getElementById("itemType").value),
            itemNameId: 0,
            itemNameText: document.getElementById("itemNameInput").value.trim(),
            categoryId: parseInt(document.getElementById("itemCategory").value),
            amount: parseFloat(document.getElementById("itemAmount").value),
            transactionDate: fromDateInput(document.getElementById("itemDate").value.trim()),
            note: document.getElementById("itemNote").value.trim() || null,
            isRecurring: document.getElementById("itemRecurring").checked,
            linkedBudgetIds: linkedIds,
            linkedBudgetNames: []
        };

        const result = await SiteUtils.postJson("/BudgetApi?handler=SaveItem", payload);
        if (result.success) {
            SiteUtils.showToast("Item saved.");
            await renderBudgetEdit(budgetId);
        } else {
            SiteUtils.showToast(result.error || "Failed to save item.", true);
        }
    }

    async function deleteItem(itemId) {
        if (!confirm("Delete this budget item?")) return;
        const result = await SiteUtils.postJson("/BudgetApi?handler=DeleteItem", { id: itemId });
        if (result.success) {
            SiteUtils.showToast("Item deleted.");
            await renderBudgetEdit(_currentBudgetId);
        } else {
            SiteUtils.showToast("Failed to delete item.", true);
        }
    }

    function showLinkedBudgets(itemId) {
        const found = _currentItems.find(function(i) { return i.id === itemId; });
        if (!found) return;

        // Build the full list: primary budget first, then all additional links
        const allNames = [];
        if (found.primaryBudgetName) allNames.push(found.primaryBudgetName);
        (found.linkedBudgetNames || []).forEach(function(n) {
            if (n && !allNames.includes(n)) allNames.push(n);
        });

        const body = document.getElementById("linkedBudgetsModalBody");
        body.innerHTML = allNames.length === 0
            ? "<p class='text-muted small mb-0'>No budget links found.</p>"
            : "<ul class='list-unstyled mb-0'>" +
              allNames.map(function(n, idx) {
                  var icon = idx === 0
                      ? "<i class='bi bi-journals me-2 text-primary'></i>"
                      : "<i class='bi bi-link-45deg me-2 text-info'></i>";
                  return "<li class='py-1'>" + icon + escHtml(n) + (idx === 0 ? " <span class='badge bg-primary-subtle text-primary small ms-1'>Primary</span>" : "") + "</li>";
              }).join("") + "</ul>";
        new bootstrap.Modal(document.getElementById("linkedBudgetsModal")).show();
    }

    function _findItem(id) {
        return _currentItems.find(function(i) { return i.id === id; }) || null;
    }

    function toDateInput(mmddyyyy) {
        if (!mmddyyyy) return "";
        var p = mmddyyyy.split("-");
        return p.length === 3 ? (p[2] + "-" + p[0] + "-" + p[1]) : mmddyyyy;
    }

    function fromDateInput(yyyymmdd) {
        if (!yyyymmdd) return "";
        var p = yyyymmdd.split("-");
        return p.length === 3 ? (p[1] + "-" + p[2] + "-" + p[0]) : yyyymmdd;
    }

    function startEditBudgetName(budgetId) {
        var label = document.getElementById("budgetEditModalLabel");
        var span = label.querySelector("#budgetNameDisplay");
        var current = span ? span.textContent : "";
        label.innerHTML =
            "<input type='text' class='form-control form-control-sm d-inline-block' id='budgetNameEdit'" +
            " maxlength='25' value='" + escHtml(current) + "' style='width:200px' />" +
            " <button class='btn btn-sm btn-success ms-1' onclick='BudgetModule.saveBudgetName(" + budgetId + ")'><i class='bi bi-check-lg'></i></button>" +
            " <button class='btn btn-sm btn-secondary ms-1' onclick='BudgetModule.renderBudgetEdit(" + budgetId + ")'><i class='bi bi-x-lg'></i></button>";
        var input = document.getElementById("budgetNameEdit");
        input.focus();
        input.select();
    }

    async function saveBudgetName(budgetId) {
        var input = document.getElementById("budgetNameEdit");
        if (!input) return;
        var newName = input.value.trim();
        if (!newName) { SiteUtils.showToast("Budget name cannot be empty.", true); return; }
        var result = await SiteUtils.postJson("/BudgetApi?handler=RenameBudget", { id: budgetId, name: newName });
        if (result.success) {
            SiteUtils.showToast("Budget renamed.");
            await renderBudgetEdit(budgetId);
        } else {
            SiteUtils.showToast(result.error || "Failed to rename budget.", true);
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

    async function openAddItemDialog() {
        getAddItemModal().show();

        const body = document.getElementById("addItemModalBody");
        body.innerHTML = spinner();

        const [categories, allBudgets] = await Promise.all([
            SiteUtils.getJson("/BudgetApi?handler=Categories"),
            SiteUtils.getJson("/BudgetApi?handler=AllBudgets")
        ]);

        _categories = categories;
        _allBudgets = allBudgets;
        _currentBudget = null;
        _currentItems = [];

        const today = new Date();
        const defaultDateInput = today.toISOString().split("T")[0];

        const catOptions = _categories.map(c =>
            `<option value="${c.id}">${escHtml(c.name)}</option>`
        ).join("");

        const budgetCheckboxes = _allBudgets.map(b =>
            `<div class="form-check">
                <input class="form-check-input" type="checkbox" id="qalink_${b.id}" value="${b.id}" />
                <label class="form-check-label small" for="qalink_${b.id}">${escHtml(b.name)}</label>
            </div>`
        ).join("");

        body.innerHTML = `
            <div class="card border-primary mb-3">
                <div class="card-body">
                    <form onsubmit="BudgetModule.saveQuickAddItem(event)">
                        <div class="row g-2 mb-2">
                            <div class="col-sm-3">
                                <label class="form-label small fw-semibold">Type <span class="text-danger">*</span></label>
                                <select class="form-select form-select-sm" id="qaItemType" required>
                                    <option value="1">Expense</option>
                                    <option value="2">Earnings</option>
                                </select>
                            </div>
                            <div class="col-sm-4 position-relative">
                                <label class="form-label small fw-semibold">Name <span class="text-danger">*</span></label>
                                <input type="text" class="form-control form-control-sm" id="qaItemNameInput"
                                       maxlength="25" autocomplete="off"
                                       oninput="BudgetModule.searchQaItemNames(this.value)"
                                       onblur="setTimeout(()=>document.getElementById('qaItemNameDropdown').innerHTML='',200)"
                                       required />
                                <div id="qaItemNameDropdown" class="autocomplete-list"></div>
                            </div>
                            <div class="col-sm-3">
                                <label class="form-label small fw-semibold">Category <span class="text-danger">*</span></label>
                                <select class="form-select form-select-sm" id="qaItemCategory" required>
                                    <option value="">Select...</option>
                                    ${catOptions}
                                </select>
                            </div>
                            <div class="col-sm-2">
                                <label class="form-label small fw-semibold">Amount <span class="text-danger">*</span></label>
                                <input type="number" class="form-control form-control-sm" id="qaItemAmount"
                                       step="0.01" min="0.01" required />
                            </div>
                        </div>
                        <div class="row g-2 mb-2">
                            <div class="col-sm-3">
                                <label class="form-label small fw-semibold">Date <span class="text-danger">*</span></label>
                                <input type="date" class="form-control form-control-sm" id="qaItemDate"
                                       value="${defaultDateInput}" required />
                            </div>
                            <div class="col-sm-6">
                                <label class="form-label small fw-semibold">Note</label>
                                <input type="text" class="form-control form-control-sm" id="qaItemNote" maxlength="500" />
                            </div>
                            <div class="col-sm-3">
                                <label class="form-label small fw-semibold">Include In</label>
                                <div class="border rounded p-2 bg-white" style="max-height:120px;overflow-y:auto;">
                                    ${budgetCheckboxes || "<span class='text-muted small'>No other budgets</span>"}
                                </div>
                            </div>
                        </div>
                        <div class="d-flex gap-2 mt-2">
                            <button type="submit" class="btn btn-primary btn-sm"><i class="bi bi-save me-1"></i>Save</button>
                            <button type="button" class="btn btn-secondary btn-sm" data-bs-dismiss="modal">Cancel</button>
                        </div>
                        <div class="form-check mt-2">
                            <input class="form-check-input" type="checkbox" id="qaItemRecurring" />
                            <label class="form-check-label small" for="qaItemRecurring">
                                <i class="bi bi-arrow-repeat me-1 text-success"></i>
                                <strong>Recurring</strong> — automatically copy to new monthly budgets
                            </label>
                        </div>
                    </form>
                </div>
            </div>`;
    }

    async function saveQuickAddItem(evt) {
        evt.preventDefault();

        const dateStr = fromDateInput(document.getElementById("qaItemDate").value.trim());

        const budget = await SiteUtils.getJson(`/BudgetApi?handler=ResolveBudgetByDate&date=${encodeURIComponent(dateStr)}`);
        if (!budget || !budget.id) {
            SiteUtils.showToast("Could not resolve monthly budget for the selected date.", true);
            return;
        }

        const linkedIds = Array.from(document.querySelectorAll("[id^='qalink_']:checked"))
            .map(cb => parseInt(cb.value));

        const payload = {
            id: 0,
            budgetId: budget.id,
            type: parseInt(document.getElementById("qaItemType").value),
            itemNameId: 0,
            itemNameText: document.getElementById("qaItemNameInput").value.trim(),
            categoryId: parseInt(document.getElementById("qaItemCategory").value),
            amount: parseFloat(document.getElementById("qaItemAmount").value),
            transactionDate: dateStr,
            note: document.getElementById("qaItemNote").value.trim() || null,
            isRecurring: document.getElementById("qaItemRecurring").checked,
            linkedBudgetIds: linkedIds,
            linkedBudgetNames: []
        };

        const result = await SiteUtils.postJson("/BudgetApi?handler=SaveItem", payload);
        if (result.success) {
            SiteUtils.showToast(`Item saved to ${escHtml(budget.name)}.`);
            getAddItemModal().hide();
            location.reload();
        } else {
            SiteUtils.showToast(result.error || "Failed to save item.", true);
        }
    }

    async function searchQaItemNames(query) {
        const dropdown = document.getElementById("qaItemNameDropdown");
        if (!query || query.length < 1) { dropdown.innerHTML = ""; return; }
        const results = await SiteUtils.getJson(`/ItemNameApi?handler=Search&q=${encodeURIComponent(query)}`);
        if (!results || !results.length) { dropdown.innerHTML = ""; return; }
        dropdown.innerHTML = results.map(r =>
            `<div class="ac-item" onmousedown="BudgetModule.selectQaItemName('${escHtml(r.name)}')">${escHtml(r.name)}</div>`
        ).join("");
    }

    function selectQaItemName(name) {
        document.getElementById("qaItemNameInput").value = name;
        document.getElementById("qaItemNameDropdown").innerHTML = "";
    }

    // Utility Modal
    const UtilityModule = (() => {
        let _utilModal = null;

        function getModal() {
            if (!_utilModal) _utilModal = new bootstrap.Modal(document.getElementById("utilityModal"));
            return _utilModal;
        }

        function openUtility() {
            getModal().show();
            document.getElementById("utilityModalBody").innerHTML = `
                <p class="text-muted mb-3">Use the button below to import data from a file.</p>
                <form id="importForm" onsubmit="UtilityModule.submitImport(event)">
                    <div class="mb-3">
                        <label class="form-label fw-semibold">Select File</label>
                        <input type="file" class="form-control" id="importFile" required />
                    </div>
                    <button type="submit" class="btn btn-primary">
                        <i class="bi bi-upload me-1"></i>Import
                    </button>
                </form>
                <div id="importResult" class="mt-3"></div>`;
        }

        async function submitImport(evt) {
            evt.preventDefault();
            const fileInput = document.getElementById("importFile");
            if (!fileInput.files.length) return;

            const formData = new FormData();
            formData.append("file", fileInput.files[0]);

            const resp = await fetch("/UtilityApi?handler=Import", {
                method: "POST",
                body: formData
            });
            const result = await resp.json();
            const resultDiv = document.getElementById("importResult");
            resultDiv.innerHTML = `<div class="alert ${result.success ? "alert-success" : "alert-danger"}">${result.message || result.error}</div>`;
        }

        return { openUtility, submitImport };
    })();

    // Expose UtilityModule globally
    window.UtilityModule = UtilityModule;

    return {
        openBudgetList,
        openBudgetEdit,
        renderBudgetEdit,
        showCreateForm,
        createBudget,
        showItemForm,
        searchItemNames,
        selectItemName,
        saveItem,
        deleteItem,
        showLinkedBudgets,
        startEditBudgetName,
        saveBudgetName,
        openAddItemDialog,
        saveQuickAddItem,
        searchQaItemNames,
        selectQaItemName
    };
})();
