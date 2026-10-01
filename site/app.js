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

const fixtureSelect = document.getElementById("comparison-fixture");
if (fixtureSelect) {
  const dataRequest = fetch("assets/package-comparison.json").then((response) => {
    if (!response.ok) throw new Error("Comparison data unavailable");
    return response.json();
  });
  dataRequest.catch(() => { fixtureSelect.disabled = true; });
  fixtureSelect.addEventListener("change", async () => {
    try {
      const data = await dataRequest;
      const workload = data.workloads.find((item) => item.name === fixtureSelect.value);
      const rows = data.matching.filter((item) => item.workload === workload.name).sort((a, b) => a.meanNs - b.meanNs);
      const peak = 1000 / rows[0].meanNs;
      document.getElementById("chart-workload").textContent = workload.label;
      document.getElementById("chart-pattern").textContent = workload.pattern;
      document.getElementById("chart-rows").replaceChildren(...rows.map((row) => {
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
}
