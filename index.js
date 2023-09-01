import * as Turbo from "@hotwired/turbo"
import { Application } from "@hotwired/stimulus"
import { definitionsFromContext } from "@hotwired/stimulus-webpack-helpers"

const application = Application.start()
const context = require.context("./src/stimulus_controllers", true, /\.js$/)
application.load(definitionsFromContext(context))

////application.debug = true;