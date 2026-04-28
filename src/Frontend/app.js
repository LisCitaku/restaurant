const state = {
    token: null,
    profile: null,
    users: [],
    waiters: [],
    tables: [],
    products: [],
    adminProducts: [],
    orders: [],
    salesSummary: null,
    publicMenuCategory: "All",
    currentView: "overview",
    selectedTableId: null,
    cart: {},
    productSizeSelections: {},
    activeServiceCategory: "All",
    refreshTimer: null
};

const elements = {
    loginScreen: document.getElementById("loginScreen"),
    dashboard: document.getElementById("dashboard"),
    loginForm: document.getElementById("loginForm"),
    username: document.getElementById("username"),
    password: document.getElementById("password"),
    loginMessage: document.getElementById("loginMessage"),
    sessionBadge: document.getElementById("sessionBadge"),
    sidebarIdentity: document.getElementById("sidebarIdentity"),
    viewTitle: document.getElementById("viewTitle"),
    headerRoleBadge: document.getElementById("headerRoleBadge"),
    logoutBtn: document.getElementById("logoutBtn"),
    topLogoutBtn: document.getElementById("topLogoutBtn"),
    userForm: document.getElementById("userForm"),
    userName: document.getElementById("userName"),
    userUsername: document.getElementById("userUsername"),
    userPassword: document.getElementById("userPassword"),
    userRole: document.getElementById("userRole"),
    userMessage: document.getElementById("userMessage"),
    usersList: document.getElementById("usersList"),
    productForm: document.getElementById("productForm"),
    editingProductId: document.getElementById("editingProductId"),
    productName: document.getElementById("productName"),
    productCategory: document.getElementById("productCategory"),
    productDescription: document.getElementById("productDescription"),
    productImageUrl: document.getElementById("productImageUrl"),
    productImageFile: document.getElementById("productImageFile"),
    uploadProductImageBtn: document.getElementById("uploadProductImageBtn"),
    productImageDropzone: document.getElementById("productImageDropzone"),
    productImagePreview: document.getElementById("productImagePreview"),
    productPrice: document.getElementById("productPrice"),
    productHasSizes: document.getElementById("productHasSizes"),
    productSizesPanel: document.getElementById("productSizesPanel"),
    productSizeS: document.getElementById("productSizeS"),
    productSizeM: document.getElementById("productSizeM"),
    productSizeL: document.getElementById("productSizeL"),
    productActive: document.getElementById("productActive"),
    productMessage: document.getElementById("productMessage"),
    adminProductsList: document.getElementById("adminProductsList"),
    tableForm: document.getElementById("tableForm"),
    tableName: document.getElementById("tableName"),
    tableSeats: document.getElementById("tableSeats"),
    tableMessage: document.getElementById("tableMessage"),
    adminTablesList: document.getElementById("adminTablesList"),
    waiterTablesList: document.getElementById("waiterTablesList"),
    serviceTablesList: document.getElementById("serviceTablesList"),
    tableSelect: document.getElementById("tableSelect"),
    selectedTableBanner: document.getElementById("selectedTableBanner"),
    serviceCategoryTabs: document.getElementById("serviceCategoryTabs"),
    serviceCategoryCount: document.getElementById("serviceCategoryCount"),
    productChecklist: document.getElementById("productChecklist"),
    cartCount: document.getElementById("cartCount"),
    cartItems: document.getElementById("cartItems"),
    cartTotal: document.getElementById("cartTotal"),
    orderForm: document.getElementById("orderForm"),
    orderNotes: document.getElementById("orderNotes"),
    orderMessage: document.getElementById("orderMessage"),
    waiterOrdersList: document.getElementById("waiterOrdersList"),
    publicMenuTabs: document.getElementById("publicMenuTabs"),
    publicMenuList: document.getElementById("publicMenuList"),
    ordersList: document.getElementById("ordersList"),
    receiptMessage: document.getElementById("receiptMessage"),
    dailySalesTotal: document.getElementById("dailySalesTotal"),
    dailySalesList: document.getElementById("dailySalesList"),
    monthlySalesTotal: document.getElementById("monthlySalesTotal"),
    monthlySalesList: document.getElementById("monthlySalesList"),
    statTables: document.getElementById("statTables"),
    statProducts: document.getElementById("statProducts"),
    statOrders: document.getElementById("statOrders"),
    statUsers: document.getElementById("statUsers")
};

document.getElementById("refreshAllBtn").addEventListener("click", loadWorkspace);
document.getElementById("resetProductFormBtn")?.addEventListener("click", resetProductForm);
elements.productHasSizes?.addEventListener("change", toggleProductSizesPanel);
elements.uploadProductImageBtn?.addEventListener("click", handleUploadProductImage);
elements.productImageUrl?.addEventListener("input", renderProductImagePreview);
elements.productImageFile?.addEventListener("change", handleProductImageSelected);
elements.logoutBtn.addEventListener("click", logout);
elements.topLogoutBtn.addEventListener("click", logout);
document.querySelectorAll(".nav-link").forEach((button) => button.addEventListener("click", () => setView(button.dataset.view)));
elements.loginForm.addEventListener("submit", handleLogin);
elements.userForm?.addEventListener("submit", handleCreateUser);
elements.productForm?.addEventListener("submit", handleSaveProduct);
elements.tableForm?.addEventListener("submit", handleCreateTable);
elements.orderForm.addEventListener("submit", handleCreateOrder);
bindImageDropzone();

async function handleLogin(event) {
    event.preventDefault();

    try {
        const result = await api("/api/auth/login", {
            method: "POST",
            body: JSON.stringify({
                username: elements.username.value,
                password: elements.password.value
            })
        }, false);

        state.token = result.token;
        state.profile = result;
        state.selectedTableId = null;
        state.cart = {};
        state.productSizeSelections = {};
        state.activeServiceCategory = "All";
        state.publicMenuCategory = "All";

        elements.loginScreen.classList.add("hidden");
        elements.dashboard.classList.remove("hidden");
        elements.sessionBadge.textContent = `${result.name} - ${result.role}`;
        elements.sessionBadge.className = "pill";
        elements.headerRoleBadge.textContent = result.role;
        elements.sidebarIdentity.textContent = `${result.name} is signed in to manage restaurant operations.`;
        elements.loginMessage.textContent = "Console loaded successfully.";

        updateRoleVisibility();
        setView(userIsAdmin() ? "overview" : "service");
        await loadWorkspace();
        startRealtimeRefresh();
    } catch (error) {
        elements.loginMessage.textContent = error.message;
    }
}

async function handleCreateUser(event) {
    event.preventDefault();

    try {
        await api("/api/users", {
            method: "POST",
            body: JSON.stringify({
                name: elements.userName.value,
                username: elements.userUsername.value,
                password: elements.userPassword.value,
                role: elements.userRole.value
            })
        });

        elements.userMessage.textContent = "User created successfully.";
        elements.userForm.reset();
        await loadAdminData();
        updateStats();
    } catch (error) {
        elements.userMessage.textContent = error.message;
    }
}

async function handleSaveProduct(event) {
    event.preventDefault();

    const payload = {
        name: elements.productName.value,
        category: elements.productCategory.value,
        description: elements.productDescription.value,
        imageUrl: elements.productImageUrl.value,
        price: Number(elements.productPrice.value),
        sizeOptions: getProductSizeOptionsFromForm(),
        isActive: elements.productActive.checked
    };

    try {
        if (elements.editingProductId.value) {
            await api(`/api/products/${elements.editingProductId.value}`, {
                method: "PUT",
                body: JSON.stringify(payload)
            });
            elements.productMessage.textContent = "Menu item updated.";
        } else {
            await api("/api/products", {
                method: "POST",
                body: JSON.stringify(payload)
            });
            elements.productMessage.textContent = "Menu item created.";
        }

        resetProductForm();
        await loadProductsForCurrentRole();
        updateStats();
    } catch (error) {
        elements.productMessage.textContent = error.message;
    }
}

async function handleCreateTable(event) {
    event.preventDefault();

    try {
        await api("/api/tables", {
            method: "POST",
            body: JSON.stringify({
                name: elements.tableName.value,
                seats: Number(elements.tableSeats.value)
            })
        });

        elements.tableMessage.textContent = "Table created successfully.";
        elements.tableForm.reset();
        elements.tableSeats.value = 4;
        await loadTables();
        updateStats();
    } catch (error) {
        elements.tableMessage.textContent = error.message;
    }
}

async function handleUploadProductImage() {
    const file = elements.productImageFile?.files?.[0];
    if (!file) {
        elements.productMessage.textContent = "Choose an image file before uploading.";
        return;
    }

    const formData = new FormData();
    formData.append("file", file);

    try {
        const response = await fetch("/api/uploads/product-image", {
            method: "POST",
            headers: {
                Authorization: `Bearer ${state.token}`
            },
            body: formData
        });

        const payload = await response.json().catch(() => ({}));
        if (!response.ok) {
            throw new Error(payload.message || "Image upload failed.");
        }

        elements.productImageUrl.value = payload.imageUrl;
        elements.productMessage.textContent = "Image uploaded successfully.";
        renderProductImagePreview();
    } catch (error) {
        elements.productMessage.textContent = error.message;
    }
}

function bindImageDropzone() {
    if (!elements.productImageDropzone) {
        return;
    }

    ["dragenter", "dragover"].forEach((eventName) => {
        elements.productImageDropzone.addEventListener(eventName, (event) => {
            event.preventDefault();
            elements.productImageDropzone.classList.add("active");
        });
    });

    ["dragleave", "dragend", "drop"].forEach((eventName) => {
        elements.productImageDropzone.addEventListener(eventName, (event) => {
            event.preventDefault();
            if (eventName !== "drop") {
                elements.productImageDropzone.classList.remove("active");
            }
        });
    });

    elements.productImageDropzone.addEventListener("drop", (event) => {
        const file = event.dataTransfer?.files?.[0];
        elements.productImageDropzone.classList.remove("active");
        if (!file || !elements.productImageFile) {
            return;
        }

        const transfer = new DataTransfer();
        transfer.items.add(file);
        elements.productImageFile.files = transfer.files;
        syncDropzoneFileState();
    });
}

function syncDropzoneFileState() {
    if (!elements.productImageDropzone || !elements.productImageFile) {
        return;
    }

    const file = elements.productImageFile.files?.[0];
    elements.productImageDropzone.innerHTML = file
        ? `<strong>${escapeForInlineHtml(file.name)}</strong><span>Ready to upload</span>`
        : `<strong>Drag & drop image here</strong><span>or use Choose File below</span>`;
}

async function handleProductImageSelected() {
    syncDropzoneFileState();

    if (!elements.productImageFile?.files?.[0]) {
        return;
    }

    await handleUploadProductImage();
}

async function handleCreateOrder(event) {
    event.preventDefault();

    const items = Object.entries(state.cart)
        .filter(([, quantity]) => quantity > 0)
        .map(([cartKey, quantity]) => {
            const { productId, sizeName } = parseCartKey(cartKey);
            return { productId, quantity, sizeName };
        });

    if (!state.selectedTableId) {
        elements.orderMessage.textContent = "Choose a table first.";
        return;
    }

    if (!items.length) {
        elements.orderMessage.textContent = "Add products with the + buttons before placing the order.";
        return;
    }

    try {
        const created = await api("/api/orders", {
            method: "POST",
            body: JSON.stringify({
                tableId: state.selectedTableId,
                notes: elements.orderNotes.value,
                items
            })
        });

        elements.orderMessage.textContent = `Order #${created.id} sent to admin and kitchen successfully.`;
        elements.orderForm.reset();
        state.cart = {};
        state.productSizeSelections = {};
        renderMenuCategories();
        renderCart();
        await Promise.all([loadTables(), loadOrders()]);
        updateStats();
    } catch (error) {
        elements.orderMessage.textContent = error.message;
    }
}

function updateRoleVisibility() {
    const isAdmin = userIsAdmin();
    const isWaiter = userIsWaiter();

    document.querySelectorAll(".admin-only").forEach((element) => {
        element.classList.toggle("hidden", !isAdmin);
    });

    document.querySelectorAll(".non-waiter-only").forEach((element) => {
        element.classList.toggle("hidden", isWaiter);
    });

    document.querySelectorAll('[data-view="overview"]').forEach((element) => {
        element.classList.toggle("hidden", isWaiter);
    });
}

function setView(view) {
    if (userIsWaiter() && !["service"].includes(view)) {
        view = "service";
    }

    state.currentView = view;
    const titleMap = {
        overview: "Dashboard",
        users: "Users",
        menu: "Menu",
        tables: "Tables",
        service: "Service",
        orders: "Orders"
    };

    elements.viewTitle.textContent = titleMap[view] || "Dashboard";
    document.querySelectorAll(".nav-link").forEach((button) => button.classList.toggle("active", button.dataset.view === view));
    document.querySelectorAll(".view-panel").forEach((panel) => panel.classList.toggle("active-view", panel.id === `view-${view}`));
}

function logout() {
    stopRealtimeRefresh();
    state.token = null;
    state.profile = null;
    state.users = [];
    state.waiters = [];
    state.tables = [];
    state.products = [];
    state.adminProducts = [];
    state.orders = [];
    state.salesSummary = null;
    state.currentView = "overview";
    state.selectedTableId = null;
    state.cart = {};
    state.productSizeSelections = {};
    state.activeServiceCategory = "All";
    state.publicMenuCategory = "All";

    elements.dashboard.classList.add("hidden");
    elements.loginScreen.classList.remove("hidden");
    elements.sessionBadge.textContent = "Offline";
    elements.sessionBadge.className = "pill muted";
    elements.headerRoleBadge.textContent = "Role";
    elements.sidebarIdentity.textContent = "Restaurant operations dashboard.";
    elements.loginMessage.textContent = "Logged out successfully.";
    elements.receiptMessage.textContent = "";
    elements.orderMessage.textContent = "";
    elements.selectedTableBanner.textContent = "Choose a table to start taking the order.";
    elements.selectedTableBanner.classList.remove("ready");
}

function startRealtimeRefresh() {
    stopRealtimeRefresh();
    state.refreshTimer = window.setInterval(async () => {
        if (!state.token) {
            return;
        }

        await Promise.all([loadTables(), loadOrders()]);
        if (userIsAdmin()) {
            await loadSalesSummary();
        }
        if (!userIsWaiter()) {
            updateStats();
        }
    }, 5000);
}

function stopRealtimeRefresh() {
    if (state.refreshTimer) {
        window.clearInterval(state.refreshTimer);
        state.refreshTimer = null;
    }
}

async function loadWorkspace() {
    if (!state.token) {
        return;
    }

    if (userIsAdmin()) {
        await Promise.all([loadAdminData(), loadSalesSummary()]);
    }

    await Promise.all([loadTables(), loadProductsForCurrentRole(), loadOrders()]);
    updateStats();
}

async function loadAdminData() {
    [state.users, state.waiters] = await Promise.all([
        api("/api/users"),
        api("/api/users/waiters")
    ]);

    renderUsers();
}

async function loadProductsForCurrentRole() {
    if (userIsAdmin()) {
        state.adminProducts = await api("/api/products/all");
        state.products = state.adminProducts.filter((product) => product.isActive);
        renderAdminProducts();
    } else {
        state.products = await api("/api/products");
    }

    ensurePublicMenuCategory();
    renderPublicMenu();
    renderPublicMenuTabs();
    ensureActiveCategory();
    renderServiceCategoryTabs();
    renderMenuCategories();
    renderCart();
}

async function loadTables() {
    state.tables = await api("/api/tables");
    if (state.selectedTableId && !state.tables.some((table) => table.id === state.selectedTableId)) {
        state.selectedTableId = null;
    }
    renderTables();
}

async function loadOrders() {
    if (userIsWaiter()) {
        state.orders = await api("/api/orders");
        renderOrders();
        return;
    }

    try {
        state.orders = await api("/api/orders/admin-feed");
    } catch (error) {
        state.orders = await api("/api/orders");
    }

    renderOrders();
}

async function loadSalesSummary() {
    if (!userIsAdmin()) {
        return;
    }

    state.salesSummary = await api("/api/orders/analytics/summary");
    renderSalesSummary();
}

function updateStats() {
    elements.statTables.textContent = state.tables.length;
    elements.statProducts.textContent = state.products.length;
    elements.statOrders.textContent = state.orders.length;
    elements.statUsers.textContent = state.users.length;
}

function renderSalesSummary() {
    if (!elements.dailySalesList || !elements.monthlySalesList) {
        return;
    }

    const summary = state.salesSummary || {
        dailyTotal: 0,
        monthlyTotal: 0,
        dailyByWaiter: [],
        monthlyByWaiter: []
    };

    elements.dailySalesTotal.textContent = formatCurrency(summary.dailyTotal);
    elements.monthlySalesTotal.textContent = formatCurrency(summary.monthlyTotal);
    elements.dailySalesList.innerHTML = renderSalesCards(summary.dailyByWaiter, "No daily waiter sales in the last 24 hours yet.");
    elements.monthlySalesList.innerHTML = renderSalesCards(summary.monthlyByWaiter, "No monthly waiter sales recorded yet.");
}

function renderSalesCards(items, emptyMessage) {
    if (!items?.length) {
        return `<article class="sales-card"><h3>No data yet</h3><p>${emptyMessage}</p></article>`;
    }

    return items.map((item) => `
        <article class="sales-card">
            <div class="sales-card-header">
                <div>
                    <h3>${item.waiterName}</h3>
                    <p>@${item.username}</p>
                </div>
                <span class="pill">${formatCurrency(item.revenue)}</span>
            </div>
            <div class="sales-metrics">
                <span class="tag">${item.ordersCount} order${item.ordersCount === 1 ? "" : "s"}</span>
                <span class="tag">Waiter ID ${item.userId}</span>
            </div>
        </article>
    `).join("");
}

function renderUsers() {
    elements.usersList.innerHTML = state.users.map((user) => `
        <article class="user-card">
            <div class="card-top">
                <div>
                    <h3>${user.name}</h3>
                    <p>${user.username}</p>
                </div>
                <span class="status ${user.isActive ? "active" : "inactive"}">${user.role}</span>
            </div>
            <div class="action-row">
                <button class="secondary toggle-user-btn" data-user-id="${user.id}" data-next-state="${!user.isActive}">
                    ${user.isActive ? "Disable" : "Enable"}
                </button>
            </div>
        </article>
    `).join("");

    document.querySelectorAll(".toggle-user-btn").forEach((button) => {
        button.addEventListener("click", async () => {
            try {
                await api(`/api/users/${button.dataset.userId}/status`, {
                    method: "PATCH",
                    body: JSON.stringify({ isActive: button.dataset.nextState === "true" })
                });
                await loadAdminData();
                await loadTables();
                updateStats();
            } catch (error) {
                elements.userMessage.textContent = error.message;
            }
        });
    });
}

function renderAdminProducts() {
    elements.adminProductsList.innerHTML = state.adminProducts.map((product) => `
        <article class="admin-menu-card">
            <div class="product-card-layout">
                ${renderProductMedia(product)}
                <div>
                    <div class="card-top">
                        <div>
                            <h3>${product.name}</h3>
                            <p>${product.category}</p>
                        </div>
                        <span class="status ${product.isActive ? "active" : "inactive"}">${product.isActive ? "Visible" : "Hidden"}</span>
                    </div>
                    <p>${product.description || "No description yet."}</p>
                    <div class="menu-meta">
                        <span class="tag">$${Number(product.price).toFixed(2)}</span>
                        ${renderSizeSummary(product)}
                    </div>
                    <div class="action-row">
                        <button class="secondary edit-product-btn" data-product-id="${product.id}">Edit</button>
                    </div>
                </div>
            </div>
        </article>
    `).join("");

    document.querySelectorAll(".edit-product-btn").forEach((button) => button.addEventListener("click", () => startEditingProduct(Number(button.dataset.productId))));
}

function renderTables() {
    elements.tableSelect.innerHTML = state.tables
        .map((table) => `<option value="${table.id}" ${table.id === state.selectedTableId ? "selected" : ""}>${table.name}</option>`)
        .join("");

    const tableCards = state.tables.length
        ? state.tables.map((table) => `
            <article class="table-card selectable ${table.id === state.selectedTableId ? "active" : ""}" data-table-id="${table.id}">
                <div class="card-top">
                    <div>
                        <h3>${table.name}</h3>
                        <p>${table.assignedWaiterName}</p>
                    </div>
                    <span class="status ${table.status.toLowerCase()}">${table.status}</span>
                </div>
                <div class="table-meta">
                    <span class="tag">${table.seats} seats</span>
                </div>
            </article>
        `).join("")
        : `<article class="table-card"><h3>No assigned tables yet</h3><p>Ask admin to assign waiter tables first.</p></article>`;

    elements.waiterTablesList.innerHTML = tableCards;
    elements.serviceTablesList.innerHTML = tableCards;
    bindTableSelection();

    if (state.selectedTableId) {
        const selected = state.tables.find((table) => table.id === state.selectedTableId);
        elements.selectedTableBanner.textContent = selected
            ? `Taking order for ${selected.name}. Tap + on menu items, then press Place Order.`
            : "Choose a table to start taking the order.";
        elements.selectedTableBanner.classList.toggle("ready", Boolean(selected));
    } else {
        elements.selectedTableBanner.textContent = "Choose a table to start taking the order.";
        elements.selectedTableBanner.classList.remove("ready");
    }

    if (userIsAdmin()) {
        elements.adminTablesList.innerHTML = state.tables.map((table) => `
            <article class="admin-table-card">
                <div class="card-top">
                    <div>
                        <h3>${table.name}</h3>
                        <p>${table.seats} seats - ${table.status}</p>
                    </div>
                    <span class="tag">${table.assignedWaiterName}</span>
                </div>
                <div class="action-row">
                    <select class="assign-waiter-select" data-table-id="${table.id}">
                        <option value="">Unassigned</option>
                        ${state.waiters.map((waiter) => `
                            <option value="${waiter.id}" ${table.assignedWaiterId === waiter.id ? "selected" : ""}>${waiter.name}</option>
                        `).join("")}
                    </select>
                </div>
            </article>
        `).join("");

        document.querySelectorAll(".assign-waiter-select").forEach((select) => {
            select.addEventListener("change", async () => {
                try {
                    await api(`/api/tables/${select.dataset.tableId}/assign`, {
                        method: "PATCH",
                        body: JSON.stringify({ waiterId: select.value ? Number(select.value) : null })
                    });
                    elements.tableMessage.textContent = "Waiter assignment updated.";
                    await loadTables();
                } catch (error) {
                    elements.tableMessage.textContent = error.message;
                }
            });
        });
    }
}

function bindTableSelection() {
    document.querySelectorAll(".table-card.selectable").forEach((card) => {
        card.addEventListener("click", () => {
            state.selectedTableId = Number(card.dataset.tableId);
            renderTables();
        });
    });
}

function renderPublicMenu() {
    const grouped = groupProductsByCategory(state.products);
    const entries = Object.entries(grouped);
    const filteredEntries = state.publicMenuCategory === "All"
        ? entries
        : entries.filter(([category]) => category === state.publicMenuCategory);

    elements.publicMenuList.innerHTML = filteredEntries.map(([category, products]) => `
        <article class="category-block product-page-card">
            <h4>${category}</h4>
            <div class="menu-grid product-page-grid">
                ${products.map((product) => `
                    <article class="menu-card">
                        <div class="product-card-layout">
                            ${renderProductMedia(product)}
                            <div>
                                <div class="card-top">
                                    <div>
                                        <h3>${product.name}</h3>
                                        <p>${product.description || "Prepared fresh for service."}</p>
                                    </div>
                                    <span class="tag">${getProductDisplayPrice(product)}</span>
                                </div>
                                ${product.sizeOptions?.length ? `<div class="size-summary">${product.sizeOptions.map((option) => `<span class="tag">${option.name} $${Number(option.price).toFixed(2)}</span>`).join("")}</div>` : ""}
                            </div>
                        </div>
                    </article>
                `).join("")}
            </div>
        </article>
    `).join("");
}

function renderPublicMenuTabs() {
    const grouped = groupProductsByCategory(state.products);
    const categories = Object.keys(grouped);
    const tabs = [
        { name: "All", count: state.products.length },
        ...categories.map((category) => ({ name: category, count: grouped[category].length }))
    ];

    if (!elements.publicMenuTabs) {
        return;
    }

    elements.publicMenuTabs.innerHTML = tabs.map((tab) => `
        <button
            type="button"
            class="service-category-tab ${state.publicMenuCategory === tab.name ? "active" : ""}"
            data-public-category="${tab.name}">
            <span>${tab.name}</span>
            <small>${tab.count}</small>
        </button>
    `).join("");

    document.querySelectorAll("[data-public-category]").forEach((button) => {
        button.addEventListener("click", () => {
            state.publicMenuCategory = button.dataset.publicCategory;
            renderPublicMenuTabs();
            renderPublicMenu();
        });
    });
}

function renderMenuCategories() {
    const grouped = groupProductsByCategory(state.products);
    const entries = Object.entries(grouped);
    const filteredEntries = state.activeServiceCategory === "All"
        ? entries
        : entries.filter(([category]) => category === state.activeServiceCategory);

    if (!filteredEntries.length) {
        elements.productChecklist.innerHTML = `
            <article class="category-block service-category-block">
                <h4>No menu items found</h4>
                <p>Ask admin to add products for this category.</p>
            </article>
        `;
        return;
    }

    elements.productChecklist.innerHTML = filteredEntries.map(([category, products]) => `
        <section class="category-block service-category-block">
            <div class="service-category-head">
                <div>
                    <h4>${category}</h4>
                    <p>${products.length} item${products.length === 1 ? "" : "s"} ready for this station</p>
                </div>
                <span class="pill muted">${category}</span>
            </div>
            <div class="menu-grid service-category-grid">
                ${products.map((product) => `
                    <article class="service-product-card">
                        <div class="product-card-layout">
                            ${renderProductMedia(product)}
                            <div>
                                <h3>${product.name}</h3>
                                <p>${product.description || product.category}</p>
                                <div class="service-product-price">
                                    <span class="tag">${getProductDisplayPrice(product)}</span>
                                </div>
                                ${renderServiceSizeSelector(product)}
                            </div>
                        </div>
                        <div class="quantity-box">
                            <button type="button" class="secondary qty-btn" data-product-id="${product.id}" data-delta="-1">-</button>
                            <span>${getCurrentQuantityForProduct(product)}</span>
                            <button type="button" class="qty-btn" data-product-id="${product.id}" data-delta="1">+</button>
                        </div>
                    </article>
                `).join("")}
            </div>
        </section>
    `).join("");

    document.querySelectorAll(".qty-btn").forEach((button) => {
        button.addEventListener("click", () => changeQuantity(Number(button.dataset.productId), Number(button.dataset.delta)));
    });

    document.querySelectorAll(".size-chip").forEach((button) => {
        button.addEventListener("click", () => {
            state.productSizeSelections[button.dataset.productId] = button.dataset.sizeName;
            renderMenuCategories();
        });
    });
}

function renderServiceCategoryTabs() {
    const grouped = groupProductsByCategory(state.products);
    const categories = Object.keys(grouped);
    const tabs = [
        { name: "All", count: state.products.length },
        ...categories.map((category) => ({ name: category, count: grouped[category].length }))
    ];

    elements.serviceCategoryCount.textContent = `${categories.length} categor${categories.length === 1 ? "y" : "ies"}`;
    elements.serviceCategoryTabs.innerHTML = tabs.map((tab) => `
        <button
            type="button"
            class="service-category-tab ${state.activeServiceCategory === tab.name ? "active" : ""}"
            data-category="${tab.name}">
            <span>${tab.name}</span>
            <small>${tab.count}</small>
        </button>
    `).join("");

    document.querySelectorAll(".service-category-tab").forEach((button) => {
        button.addEventListener("click", () => {
            state.activeServiceCategory = button.dataset.category;
            renderServiceCategoryTabs();
            renderMenuCategories();
        });
    });
}

function changeQuantity(productId, delta) {
    const product = state.products.find((entry) => entry.id === productId);
    if (!product) {
        return;
    }

    const selectedSize = getSelectedSizeName(product);
    const cartKey = buildCartKey(productId, selectedSize);
    const nextValue = Math.max(0, (state.cart[cartKey] || 0) + delta);
    if (nextValue === 0) {
        delete state.cart[cartKey];
    } else {
        state.cart[cartKey] = nextValue;
    }

    renderMenuCategories();
    renderCart();
}

function renderCart() {
    const lines = Object.entries(state.cart)
        .filter(([, quantity]) => quantity > 0)
        .map(([cartKey, quantity]) => {
            const { productId, sizeName } = parseCartKey(cartKey);
            const product = state.products.find((entry) => entry.id === productId);
            return product ? { product, quantity, sizeName, unitPrice: getUnitPrice(product, sizeName) } : null;
        })
        .filter(Boolean);

    const totalItems = lines.reduce((sum, line) => sum + line.quantity, 0);
    const totalAmount = lines.reduce((sum, line) => sum + (line.unitPrice * line.quantity), 0);

    elements.cartCount.textContent = `${totalItems} item${totalItems === 1 ? "" : "s"}`;
    elements.cartTotal.textContent = `$${totalAmount.toFixed(2)}`;
    elements.cartItems.innerHTML = lines.length
        ? lines.map((line) => `
            <article class="cart-line">
                <div>
                    <strong>${line.product.name}${line.sizeName ? ` (${line.sizeName})` : ""}</strong>
                    <div class="menu-meta">
                        <span class="tag">x${line.quantity}</span>
                        ${line.sizeName ? `<span class="tag">${line.sizeName}</span>` : ""}
                    </div>
                </div>
                <strong>$${(line.unitPrice * line.quantity).toFixed(2)}</strong>
            </article>
        `).join("")
        : `<article class="cart-line"><span>No products selected yet.</span><strong>$0.00</strong></article>`;
}

function renderOrders() {
    const waiterOrders = state.orders.filter((order) => order.createdBy === state.profile?.name);
    elements.waiterOrdersList.innerHTML = waiterOrders.length
        ? waiterOrders.map((order) => `
            <article class="order-card">
                <div class="card-top">
                    <div>
                        <h3>Order #${order.id}</h3>
                        <p>${order.tableName}</p>
                    </div>
                    <span class="status ${order.status.toLowerCase()}">${order.status}</span>
                </div>
                <p>${order.notes || "No notes."}</p>
            </article>
        `).join("")
        : `<article class="order-card"><h3>No sent orders yet</h3><p>Your submitted tickets will appear here automatically.</p></article>`;

    elements.ordersList.innerHTML = state.orders.length
        ? state.orders.map((order) => {
            const items = order.items.map((item) => `<li>${item.productName} x${item.quantity}</li>`).join("");
            const showFinancials = !userIsWaiter();
            const actions = userCanBill()
                ? `
                    <div class="action-row">
                        <button class="secondary pay-order-btn" data-order-id="${order.id}">Mark paid</button>
                        <button class="receipt-btn" data-order-id="${order.id}">Print receipt</button>
                    </div>
                `
                : "";

            return `
                <article class="order-card">
                    <div class="card-top">
                        <div>
                            <h3>Order #${order.id}</h3>
                            <p>${order.tableName} - Waiter: ${order.createdBy}</p>
                        </div>
                        <span class="status ${order.status.toLowerCase()}">${order.status}</span>
                    </div>
                    <p>${order.notes || "No special notes."}</p>
                    <ul>${items}</ul>
                    ${showFinancials ? `<div class="menu-meta"><span class="tag">$${Number(order.total).toFixed(2)}</span></div>` : ""}
                    ${actions}
                </article>
            `;
        }).join("")
        : `<article class="order-card"><h3>No open orders</h3><p>Orders will appear here in real time.</p></article>`;

    document.querySelectorAll(".pay-order-btn").forEach((button) => {
        button.addEventListener("click", async () => {
            try {
                await api(`/api/orders/${button.dataset.orderId}/pay`, { method: "POST" });
                elements.receiptMessage.textContent = `Order #${button.dataset.orderId} marked as paid.`;
                await Promise.all([loadOrders(), loadTables()]);
                updateStats();
            } catch (error) {
                elements.receiptMessage.textContent = error.message;
            }
        });
    });

    document.querySelectorAll(".receipt-btn").forEach((button) => {
        button.addEventListener("click", async () => {
            try {
                const receipt = await api(`/api/receipts/${button.dataset.orderId}`, { method: "POST" });
                elements.receiptMessage.textContent = `Receipt generated: ${receipt.filePath}`;
            } catch (error) {
                elements.receiptMessage.textContent = error.message;
            }
        });
    });
}

function startEditingProduct(productId) {
    const product = state.adminProducts.find((entry) => entry.id === productId);
    if (!product) {
        return;
    }

    setView("menu");
    elements.editingProductId.value = product.id;
    elements.productName.value = product.name;
    elements.productCategory.value = product.category;
    elements.productDescription.value = product.description;
    elements.productImageUrl.value = product.imageUrl || "";
    renderProductImagePreview();
    elements.productPrice.value = product.price;
    elements.productHasSizes.checked = Boolean(product.sizeOptions?.length);
    setSizeInputs(product.sizeOptions || []);
    toggleProductSizesPanel();
    elements.productActive.checked = product.isActive;
    elements.productMessage.textContent = `Editing ${product.name}.`;
}

function resetProductForm() {
    if (!elements.productForm) {
        return;
    }

    elements.productForm.reset();
    elements.editingProductId.value = "";
    elements.productImageUrl.value = "";
    if (elements.productImageFile) {
        elements.productImageFile.value = "";
    }
    elements.productHasSizes.checked = false;
    setSizeInputs([]);
    toggleProductSizesPanel();
    elements.productActive.checked = true;
    elements.productMessage.textContent = "";
    renderProductImagePreview();
}

function groupProductsByCategory(products) {
    return products.reduce((groups, product) => {
        const category = product.category || "Menu";
        if (!groups[category]) {
            groups[category] = [];
        }
        groups[category].push(product);
        return groups;
    }, {});
}

function ensureActiveCategory() {
    const categories = Object.keys(groupProductsByCategory(state.products));

    if (!categories.length) {
        state.activeServiceCategory = "All";
        return;
    }

    if (state.activeServiceCategory !== "All" && !categories.includes(state.activeServiceCategory)) {
        state.activeServiceCategory = categories[0];
    }
}

function ensurePublicMenuCategory() {
    const categories = Object.keys(groupProductsByCategory(state.products));

    if (!categories.length) {
        state.publicMenuCategory = "All";
        return;
    }

    if (state.publicMenuCategory !== "All" && !categories.includes(state.publicMenuCategory)) {
        state.publicMenuCategory = categories[0];
    }
}

function toggleProductSizesPanel() {
    const showSizes = elements.productHasSizes?.checked;
    elements.productSizesPanel?.classList.toggle("hidden", !showSizes);
}

function getProductSizeOptionsFromForm() {
    if (!elements.productHasSizes?.checked) {
        return [];
    }

    return [
        { name: "S", price: Number(elements.productSizeS.value) },
        { name: "M", price: Number(elements.productSizeM.value) },
        { name: "L", price: Number(elements.productSizeL.value) }
    ].filter((option) => option.price > 0);
}

function setSizeInputs(sizeOptions) {
    const sizeMap = Object.fromEntries((sizeOptions || []).map((option) => [option.name, option.price]));
    elements.productSizeS.value = sizeMap.S || "";
    elements.productSizeM.value = sizeMap.M || "";
    elements.productSizeL.value = sizeMap.L || "";
}

function renderSizeSummary(product) {
    if (!product.sizeOptions?.length) {
        return "";
    }

    return product.sizeOptions
        .map((option) => `<span class="tag">${option.name} $${Number(option.price).toFixed(2)}</span>`)
        .join("");
}

function renderServiceSizeSelector(product) {
    if (!product.sizeOptions?.length) {
        return "";
    }

    const selectedSize = getSelectedSizeName(product);
    return `
        <div class="size-selector">
            ${product.sizeOptions.map((option) => `
                <button
                    type="button"
                    class="size-chip ${selectedSize === option.name ? "active" : ""}"
                    data-product-id="${product.id}"
                    data-size-name="${option.name}">
                    <span>${option.name}</span>
                    <small>$${Number(option.price).toFixed(2)}</small>
                </button>
            `).join("")}
        </div>
    `;
}

function renderProductMedia(product) {
    const safeName = product.name || "Menu";
    if (product.imageUrl) {
        return `
            <div class="product-media">
                <img src="${product.imageUrl}" alt="${safeName}" onerror="this.parentElement.innerHTML='${escapeForInlineHtml(getProductInitials(safeName))}'">
            </div>
        `;
    }

    return `<div class="product-media">${getProductInitials(safeName)}</div>`;
}

function renderProductImagePreview() {
    if (!elements.productImagePreview) {
        return;
    }

    const imageUrl = elements.productImageUrl?.value?.trim();
    if (!imageUrl) {
        elements.productImagePreview.classList.add("hidden");
        elements.productImagePreview.innerHTML = "";
        return;
    }

    elements.productImagePreview.classList.remove("hidden");
    elements.productImagePreview.innerHTML = `
        <img src="${imageUrl}" alt="Product preview">
        <div>
            <strong>Image ready</strong>
            <p>${imageUrl}</p>
        </div>
    `;
}

function getProductInitials(name) {
    return name
        .split(" ")
        .filter(Boolean)
        .slice(0, 2)
        .map((part) => part[0]?.toUpperCase() || "")
        .join("");
}

function escapeForInlineHtml(value) {
    return String(value)
        .replace(/&/g, "&amp;")
        .replace(/</g, "&lt;")
        .replace(/>/g, "&gt;")
        .replace(/"/g, "&quot;")
        .replace(/'/g, "&#39;");
}

function getSelectedSizeName(product) {
    if (!product.sizeOptions?.length) {
        return "";
    }

    return state.productSizeSelections[product.id] || product.sizeOptions[0].name;
}

function getCurrentQuantityForProduct(product) {
    const sizeName = getSelectedSizeName(product);
    return state.cart[buildCartKey(product.id, sizeName)] || 0;
}

function getProductDisplayPrice(product) {
    if (!product.sizeOptions?.length) {
        return `$${Number(product.price).toFixed(2)}`;
    }

    const lowestPrice = Math.min(...product.sizeOptions.map((option) => Number(option.price)));
    return `From $${lowestPrice.toFixed(2)}`;
}

function getUnitPrice(product, sizeName) {
    if (!product.sizeOptions?.length) {
        return Number(product.price);
    }

    const selectedOption = product.sizeOptions.find((option) => option.name === sizeName) || product.sizeOptions[0];
    return Number(selectedOption.price);
}

function buildCartKey(productId, sizeName = "") {
    return `${productId}|${sizeName || ""}`;
}

function parseCartKey(cartKey) {
    const [productId, sizeName = ""] = cartKey.split("|");
    return {
        productId: Number(productId),
        sizeName
    };
}

function userIsAdmin() {
    return state.profile && ["Admin", "Manager"].includes(state.profile.role);
}

function userIsWaiter() {
    return state.profile && state.profile.role === "Waiter";
}

function userCanBill() {
    return state.profile && ["Admin", "Manager", "Cashier"].includes(state.profile.role);
}

function formatCurrency(value) {
    return `$${Number(value || 0).toFixed(2)}`;
}

async function api(url, options = {}, authenticated = true) {
    const config = {
        method: options.method || "GET",
        headers: {
            "Content-Type": "application/json",
            ...(options.headers || {})
        }
    };

    if (authenticated && state.token) {
        config.headers.Authorization = `Bearer ${state.token}`;
    }

    if (options.body) {
        config.body = options.body;
    }

    const response = await fetch(url, config);
    const payload = await response.json().catch(() => ({}));

    if (!response.ok) {
        throw new Error(payload.message || `Request failed with status ${response.status}.`);
    }

    return payload;
}
