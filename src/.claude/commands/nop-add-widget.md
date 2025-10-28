---
name: /nop-add-widget
description: Add a widget to nopCommerce plugin for displaying UI in public store or admin
---

# Add Widget to nopCommerce Plugin

You are adding a widget to display custom UI in nopCommerce public store or admin area. Classify this request:

**This is a COMPLEX MISSION** because:
- Multiple components (IWidgetPlugin, ViewComponent, views, JavaScript/CSS)
- UI/UX decisions required
- Multiple agents needed (nopcommerce-widget-specialist, nopcommerce-ui-specialist)

## Action Required

**Immediately delegate to mission-commander** for blueprint creation.

## Information to Gather from User (via mission-commander)

Ask the user for:

1. **Widget Purpose**:
   - What should the widget display?
   - Static content or dynamic data?
   - Third-party integration (Google Analytics, Facebook Pixel, etc.)?

2. **Widget Placement**:
   - Which widget zones? (homepage, product details, footer, etc.)
   - Public store or admin area?
   - Multiple zones or single zone?

3. **Data Requirements**:
   - What data does the widget need?
   - Should data be cached?
   - Does it need to call APIs?

4. **UI/UX Requirements**:
   - Does it need custom JavaScript?
   - Does it need custom CSS?
   - Should it be responsive?
   - Does user need to configure appearance?

## Delegation Command

Use the Task tool to delegate to mission-commander:

```
Create a comprehensive mission blueprint for adding a widget to nopCommerce:

**Widget Specification:**
- Widget Name: [WidgetName]
- Purpose: [What the widget does]
- Plugin: Nop.Plugin.Widgets.[WidgetName]

**Placement:**
- Target Zones: [List of PublicWidgetZones or AdminWidgetZones]
  Examples:
  - PublicWidgetZones.HomepageTop
  - PublicWidgetZones.ProductDetailsTop
  - PublicWidgetZones.Footer

**Functionality:**
[Describe what the widget displays and how it works]

**Data Requirements:**
- Data source: [nopCommerce services, external API, static content]
- Caching needed: [Yes/No]
- Cache duration: [time]

**UI Requirements:**
- JavaScript needed: [Yes/No - describe functionality]
- CSS needed: [Yes/No - custom styling requirements]
- Responsive design: [Yes - mobile-first]
- Bootstrap components: [List components to use]

**Configuration:**
- Enable/disable toggle
- Customizable settings: [list]
- Multi-store support: [Yes/No]
- Display order configuration

**Deliverables:**
1. IWidgetPlugin implementation ({WidgetName}Plugin.cs)
2. ViewComponent ({WidgetName}ViewComponent.cs)
3. ViewComponent view(s)
4. Admin configuration controller
5. Admin configuration view
6. JavaScript files (if needed)
7. CSS files (if needed)
8. Settings model
9. Localization resources
10. Installation/uninstallation logic

**Agent Assignment:**
- nopcommerce-widget-specialist: IWidgetPlugin, ViewComponent, widget zones
- nopcommerce-ui-specialist: Views, JavaScript, CSS, responsive design
- nopcommerce-plugin-developer: Settings, configuration, localization

**Quality Standards:**
- Widget performance (no page slowdown)
- Responsive on all devices
- Cross-browser compatibility
- No JavaScript errors
- Follows nopCommerce UI patterns
- Multi-store configuration support
- Zero compiler warnings

Ensure the widget follows nopCommerce 4.90 standards and provides excellent UX.
```

## Expected Outcome

Mission-commander creates blueprint → Team Commander executes → Complete widget implementation with ViewComponent, views, and configuration.
