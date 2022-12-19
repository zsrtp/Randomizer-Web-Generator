'use strict';

(function () {
  function removetooltips() {
    document.querySelectorAll('.tooltip').forEach((tooltip) => {
      tooltip.remove();
    });
  }

  const tooltipElements = document.querySelectorAll('[data-tooltip-text]');
  tooltipElements.forEach((tooltipElement) => {
    tooltipElement.addEventListener('mouseout', removetooltips);

    tooltipElement.addEventListener('mouseover', (e) => {
      const { currentTarget: target } = e;

      const newEl = document.createElement('div');
      newEl.classList.add('tooltip');

      const tooltipContent = target.getAttribute('data-tooltip-text');
      newEl.textContent = tooltipContent;

      const coords = target.getBoundingClientRect();
      if (target.getAttribute('data-tooltip-position') === 'left') {
        newEl.style.right = window.innerWidth - 5 - coords.left + 'px';
      } else {
        newEl.style.left = coords.left + e.currentTarget.clientWidth + 5 + 'px';
      }
      newEl.style.top = coords.top + 'px';

      document.body.appendChild(newEl);
      $(newEl).hide().fadeIn(300);
    });
  });
})();
