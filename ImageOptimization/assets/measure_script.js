if (document.URL.includes("Detail")) {
  window.addEventListener("load", function () {

    var whatever = [];

    window.performance.getEntriesByType('resource').map(item => {
      item.initiatorType === 'img' && whatever.push(item);
      //console.log(item);
    });

    whatever.forEach(function (entry) {
      var duration = entry.responseEnd - entry.requestStart;
      duration = duration.toString().replace(/\./g, ',')
      var file = entry.name.substring(entry.name.lastIndexOf('/') + 1);
      console.log(`File: ${file}, Duration: ${duration} ms`)

      var element = document.querySelector(`span[data-name="${file}"]`);

      element.textContent = duration;
      //console.log(element);
    });
  });
}