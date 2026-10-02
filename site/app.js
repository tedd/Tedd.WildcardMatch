document.querySelectorAll("[data-copy-target]").forEach((button) => {
  button.addEventListener("click", async () => {
    const source = document.getElementById(button.dataset.copyTarget);
    if (!source) return;
    const original = button.textContent;
    try {
      await navigator.clipboard.writeText(source.textContent.trim());
      button.textContent = "Copied";
    } catch {
      const selection = window.getSelection();
      selection?.removeAllRanges();
      const range = document.createRange();
      range.selectNodeContents(source);
      selection?.addRange(range);
      button.textContent = "Selected";
    }
    window.setTimeout(() => { button.textContent = original; }, 1800);
  });
});

const comparisonCharts = document.querySelectorAll("[data-comparison-chart]");
if (comparisonCharts.length) {
  const dataRequest = fetch("assets/package-comparison.json?v=2.2.0").then((response) => {
    if (!response.ok) throw new Error("Comparison data unavailable");
    return response.json();
  });
  comparisonCharts.forEach((chart) => {
    const fixtureSelect = chart.querySelector("[data-fixture-select]");
    dataRequest.catch(() => { fixtureSelect.disabled = true; });
    fixtureSelect.addEventListener("change", async () => {
      try {
        const data = await dataRequest;
        const workload = data.workloads.find((item) => item.name === fixtureSelect.value && item.inputGroup === chart.dataset.comparisonChart);
        const rows = data.matching.filter((item) => item.workload === workload.name).sort((a, b) => a.meanNs - b.meanNs);
        const peak = 1000 / rows[0].meanNs;
        const direct = rows.find((row) => row.library === "TeddDirectReused");
        const glob = rows.find((row) => row.library === "DotNetGlob");
        chart.querySelector("[data-direct-speedup]").textContent = `${(glob.meanNs / direct.meanNs).toFixed(2)}×`;
        chart.querySelector("[data-chart-workload]").textContent = workload.label;
        chart.querySelector("[data-chart-pattern]").textContent = workload.pattern;
        chart.querySelector("[data-chart-rows]").replaceChildren(...rows.map((row) => {
          const own = row.library.startsWith("Tedd");
          const rate = 1000 / row.meanNs;
          const element = document.createElement("div");
          element.className = `bar-row${own ? " comparison-ours" : ""}`;
          const name = document.createElement("span");
          name.textContent = data.libraries[row.library].label;
          const track = document.createElement("div");
          track.className = "bar-track";
          const bar = document.createElement("i");
          bar.className = `bar ${own ? "ours" : "other"}`;
          bar.style.width = `${rate / peak * 100}%`;
          track.append(bar);
          const value = document.createElement("b");
          value.textContent = rate.toFixed(2);
          element.append(name, track, value);
          return element;
        }));
      } catch {
        fixtureSelect.disabled = true;
      }
    });
  });
}
