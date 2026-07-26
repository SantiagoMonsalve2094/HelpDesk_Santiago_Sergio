/**
 * @typedef {"low"|"medium"|"high"|"critical"} TicketPriority
 * @typedef {"open"|"assigned"|"inProgress"|"resolved"|"closed"|"reopened"|"overdue"} TicketStatus
 * @typedef {"user"|"technician"|"supervisor"|"superAdmin"} UserRole
 *
 * @typedef {Object} Ticket
 * @property {string} id
 * @property {string} ticketNumber
 * @property {string} subject
 * @property {string} description
 * @property {string} creatorUserId
 * @property {string} supportCategoryId
 * @property {TicketPriority} priority
 * @property {TicketStatus} status
 * @property {string|null} currentTechnicianUserId
 * @property {boolean} isOverdue
 * @property {string} createdAtUtc
 * @property {string} updatedAtUtc
 *
 * @typedef {Object} User
 * @property {string} id
 * @property {string} fullName
 * @property {string} email
 * @property {UserRole} role
 * @property {boolean} isActive
 *
 * @typedef {Object} SupportCategory
 * @property {string} id
 * @property {string} name
 * @property {string} description
 * @property {boolean} isActive
 *
 * @typedef {Object} SlaReport
 * @property {Array} groups
 * @property {number} totalMetCycles
 * @property {number} totalBreachedCycles
 * @property {number} totalPendingCycles
 * @property {number} totalEvaluatedCycles
 * @property {number|null} compliancePercentage
 */

export {};
