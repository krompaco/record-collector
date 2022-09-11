import { Controller } from "@hotwired/stimulus"

export default class extends Controller {
  static targets = ['button']
  static values = {
    open: { type: Boolean, default: false },
  }

  initialize() {
    this.toggleClass = 'hidden';
    this.isConnected = false;
  }

  connect() {
    this.buttonTargets.forEach(target => {
      var listId = target.getAttribute('data-aria-controls');
      target.setAttribute('aria-controls', listId);
      target.setAttribute('aria-expanded', this.openValue);

      document.getElementById(listId).classList.add(this.toggleClass);
    })

    this.isConnected = true;
  }

  toggle(event) {
    event.preventDefault();
    this.openValue = !this.openValue;
  }

  openValueChanged() {
    if (!this.isConnected) { return }

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
