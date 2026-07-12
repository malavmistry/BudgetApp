/* category.js — Category modal management */

const CategoryModule = (() => {
    let _modal = null;

    function getModal() {
        if (!_modal) {
            _modal = new bootstrap.Modal(document.getElementById("categoryModal"));
        }
        return _modal;
    }

    async function openCategoryList() {
        getModal().show();
        await renderCategoryList();
    }

    async function renderCategoryList() {
        const body = document.getElementById("categoryModalBody");
        body.innerHTML = "<div class=\"text-center py-4\"><div class=\"spinner-border text-primary\" role=\"status\"></div></div>";

        const categories = await SiteUtils.getJson("/CategoryApi?handler=List");
        _categories = categories;

        body.innerHTML = `
            <div class="d-flex justify-content-end mb-3">
                <button class="btn btn-primary btn-sm" onclick="CategoryModule.openEditForm(0)">
                    <i class="bi bi-plus-lg me-1"></i>Add Category
                </button>
            </div>
            <div id="categoryFormContainer"></div>
            <div class="table-responsive">
                <table class="table table-hover table-sm">
                    <thead class="table-light">
                        <tr>
                            <th>Name</th>
                            <th>Description</th>
                            <th>Status</th>
                            <th></th>
                        </tr>
                    </thead>
                    <tbody id="categoryTableBody">
                        ${categories.map(renderCategoryRow).join("")}
                    </tbody>
                </table>
                ${categories.length === 0 ? "<p class=\"text-muted text-center py-3\">No categories yet.</p>" : ""}
            </div>`;
    }

    function renderCategoryRow(c) {
        return `<tr>
            <td class="fw-semibold small">${escHtml(c.name)}</td>
            <td class="small text-muted">${escHtml(c.description || "")}</td>
            <td>
                <span class="badge ${c.isActive ? "bg-success-subtle text-success" : "bg-secondary-subtle text-secondary"} small">
                    ${c.isActive ? "Active" : "Inactive"}
                </span>
            </td>
            <td>
                <button class="btn btn-outline-primary btn-sm" onclick="CategoryModule.openEditForm(${c.id})">
                    <i class="bi bi-pencil"></i>
                </button>
            </td>
        </tr>`;
    }

    function openEditForm(id) {
        const categories = _getCachedCategories();
        const cat = id === 0 ? null : categories.find(c => c.id === id);

        const formHtml = `
            <div class="card border-primary mb-3">
                <div class="card-header bg-primary-subtle fw-semibold">
                    ${id === 0 ? "Add Category" : "Edit Category"}
                </div>
                <div class="card-body">
                    <form id="categoryForm" onsubmit="CategoryModule.saveCategory(event, ${id})">
                        <div class="mb-3">
                            <label class="form-label fw-semibold">Name <span class="text-danger">*</span></label>
                            <input type="text" class="form-control" id="catName" maxlength="25"
                                   value="${cat ? escHtml(cat.name) : ""}" required />
                        </div>
                        <div class="mb-3">
                            <label class="form-label fw-semibold">Description</label>
                            <textarea class="form-control" id="catDesc" maxlength="200" rows="2">${cat ? escHtml(cat.description || "") : ""}</textarea>
                        </div>
                        <div class="mb-3 form-check">
                            <input type="checkbox" class="form-check-input" id="catActive"
                                   ${cat ? (cat.isActive ? "checked" : "") : "checked"} />
                            <label class="form-check-label" for="catActive">Active</label>
                        </div>
                        <div class="d-flex gap-2">
                            <button type="submit" class="btn btn-primary btn-sm">
                                <i class="bi bi-save me-1"></i>Save
                            </button>
                            <button type="button" class="btn btn-secondary btn-sm"
                                    onclick="document.getElementById('categoryFormContainer').innerHTML=''">
                                Cancel
                            </button>
                        </div>
                    </form>
                </div>
            </div>`;

        document.getElementById("categoryFormContainer").innerHTML = formHtml;
    }

    let _categories = [];

    function _getCachedCategories() {
        return _categories;
    }

    async function saveCategory(evt, id) {
        evt.preventDefault();
        const payload = {
            id: id,
            name: document.getElementById("catName").value.trim(),
            description: document.getElementById("catDesc").value.trim() || null,
            isActive: document.getElementById("catActive").checked
        };

        const result = await SiteUtils.postJson("/CategoryApi?handler=Save", payload);
        if (result.success) {
            SiteUtils.showToast("Category saved successfully.");
            await renderCategoryList();
        } else {
            SiteUtils.showToast("Failed to save category.", true);
        }
    }

    function escHtml(text) {
        const d = document.createElement("div");
        d.textContent = text;
        return d.innerHTML;
    }

    return { openCategoryList, openEditForm, saveCategory };
})();
