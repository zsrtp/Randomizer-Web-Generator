'use strict';

(function () {
  function removeTooltips() {
    document.querySelectorAll('.tooltip').forEach((tooltip) => {
      tooltip.remove();
    });
  }

  function initTooltipsInTree(root, optionsIn) {
    const options = Object.assign(
      {
        fadeInDelay: 300,
        extraPadding: 0,
      },
      optionsIn
    );

    const tooltipElements = Array.from(
      root.querySelectorAll('[data-tooltip-text]')
    );
    if (
      typeof root.hasAttribute === 'function' &&
      root.hasAttribute('data-tooltip-text')
    ) {
      tooltipElements.push(root);
    }

    tooltipElements.forEach((tooltipElement) => {
      const $tooltipElement = $(tooltipElement);
      if ($tooltipElement.attr('data-has-tooltip') === 'true') {
        console.log('');
        console.log(tooltipElement);
        console.log('returning for ^ this el since data-has-tooltip is true');
        return;
      }

      $(tooltipElement)
        .attr('data-has-tooltip', 'true')
        .on('mouseout', removeTooltips)
        .on('mouseover', (e) => {
          const { currentTarget: target } = e;

          const newEl = document.createElement('div');
          newEl.classList.add('tooltip');

          const tooltipContent = target.getAttribute('data-tooltip-text');
          newEl.textContent = tooltipContent;

          const coords = target.getBoundingClientRect();
          let extraPadding = options.extraPadding;
          if (Number.isNaN(extraPadding) || typeof extraPadding !== 'number') {
            extraPadding = 0;
          }
          if (target.getAttribute('data-tooltip-position') === 'left') {
            newEl.style.right =
              window.innerWidth - 5 - extraPadding - coords.left + 'px';
          } else {
            newEl.style.left =
              coords.left +
              e.currentTarget.clientWidth +
              5 +
              extraPadding +
              'px';
          }
          newEl.style.top = coords.top + 'px';

          document.body.appendChild(newEl);
          $(newEl).hide().fadeIn(options.fadeInDelay);
        });
    });
  }

  window.initTooltipsInTree = initTooltipsInTree;
  window.removeTooltips = removeTooltips;
})();
