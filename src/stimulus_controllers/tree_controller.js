import { Controller } from 'stimulus'

export default class extends Controller {
  static targets = ['button']
  static values = { open: Boolean }

  initialize() {
    this.toggleClass = 'hidden';
  }

  connect() {
    this.buttonTargets.forEach(target => {
      var listId = target.getAttribute('data-aria-controls');
      target.setAttribute('aria-controls', listId);
      target.setAttribute('aria-expanded', this.openValue);
    })
  }

  toggle(event) {
    event.preventDefault();
    this.openValue = !this.openValue;
  }

  openValueChanged() {
    if (!this.toggleClass) { return }

    this.buttonTargets.forEach(target => {
      target.setAttribute('aria-expanded', this.openValue);

      var listId = target.getAttribute('data-aria-controls');
      document.getElementById(listId).classList.toggle(this.toggleClass);

      // SVGs
      target.querySelector('.js-content-expanded').classList.toggle(this.toggleClass);
      target.querySelector('.js-content-collapsed').classList.toggle(this.toggleClass);
    })
  }
}
