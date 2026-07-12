/* home.js — Home page charts */

const HomeModule = (() => {
    function initExpensePie(slices) {
        SiteUtils.buildPieChart("monthExpenseChart", slices);
    }

    function initPieChart(canvasId, slices) {
        SiteUtils.buildPieChart(canvasId, slices);
    }

    return { initExpensePie, initPieChart };
})();
