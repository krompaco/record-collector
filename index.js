import * as Turbo from "@hotwired/turbo"

import { Application } from "stimulus"
import { definitionsFromContext } from "stimulus/webpack-helpers"

const application = Application.start()
const context = require.context("./src/stimulus_controllers", true, /\.js$/)
application.load(definitionsFromContext(context))